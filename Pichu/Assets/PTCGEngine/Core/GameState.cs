using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Serialization;
using tylrpage.Pichu.PTCGEngine.Random;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class GameState
    {
        public LinkedList<GameStep> StepQueue = new();
        public ChoiceRequest PendingChoice = null;
        
        public PlayerState Player1State;
        public PlayerState Player2State;
        public CardInstance Stadium;
        public int TurnNumber;
        public bool IsPlayer1Turn;
        public GamePhase Phase;
        public TurnFlags Turn;
        public int NextCardInstanceId;
        public int NextChoiceRequestId;
        // null means game is still in progress
        public bool? Player1Won;
        public TinyRNG Rng = new();
        public bool EndTurnRequested;
        public bool Player1GoesFirst;
        
        private static readonly JsonSerializerSettings CloneSettings = new()
        {
            // Creates fresh lists for fields, for lists that are pre-filled at init
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            // Serialize types for GameStep queue
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new EngineTypeBinder(),
        };

        public IEnumerable<CardInstance> GetAllCards()
        {
            foreach (CardInstance card in CardsIn(Player1State)) yield return card;
            foreach (CardInstance card in CardsIn(Player2State)) yield return card;
            if (Stadium != null)
            {
                foreach (CardInstance card in WithAttachments(Stadium))
                {
                    yield return card;
                }
            }
        }

        private static IEnumerable<CardInstance> CardsIn(PlayerState playerState)
        {
            if (playerState.Active != null)
            {
                foreach (CardInstance card in WithAttachments(playerState.Active))
                {
                    yield return card;
                }
            }
            
            foreach (CardInstance benchCard in playerState.Bench)
            {
                foreach (CardInstance card in WithAttachments(benchCard))
                {
                    yield return card;
                }
            }
            
            // Deck, hand, discard, and prize cards don't have any attachments
            foreach (CardInstance card in playerState.Deck) yield return card;
            foreach (CardInstance card in playerState.Hand) yield return card;
            foreach (CardInstance card in playerState.Discard) yield return card;
            foreach (CardInstance card in playerState.Prize) yield return card;
        }

        private static IEnumerable<CardInstance> WithAttachments(CardInstance parent)
        {
            yield return parent;
            foreach (CardInstance attachment in OnlyAttachments(parent))
            {
                yield return attachment;
            }
        }

        private static IEnumerable<CardInstance> OnlyAttachments(CardInstance parent)
        {
            foreach (CardInstance card in parent.AttachedEnergy) yield return card;
            foreach (CardInstance card in parent.AttachedTools) yield return card;
            foreach (CardInstance card in parent.AttachedPokemon) yield return card;
        }

        public CardInstance FindCard(int cardInstanceId)
        {
            foreach (CardInstance card in GetAllCards())
            {
                if (card.Id == cardInstanceId)
                {
                    return card;
                }
            }

            return null;
        }

        public PlayerState GetPlayer(bool isPlayer1)
        {
            return isPlayer1 ? Player1State : Player2State;
        }

        public PlayerState GetCurrentTurnPlayer()
        {
            return GetPlayer(IsPlayer1Turn);
        }
        
        public GameState Clone()
        {
            // Serializes public fields, fields with [JsonProperty], and ignores fields with [JsonIgnore]
            string json = JsonConvert.SerializeObject(this, CloneSettings);
            return JsonConvert.DeserializeObject<GameState>(json, CloneSettings);
        }

        public int GetNextCardInstanceId()
        {
            // Important that we start at 1, since cards use "0" to mean no card
            return ++NextCardInstanceId;
        }
        
        public int GetNextChoiceRequest()
        {
            return ++NextChoiceRequestId;
        }
        
        private void RemoveCardFromCurrentLocation(CardInstance card)
        {
            // Children just get un-parented, zone state only tracks top level cards
            if (card.ParentCardInstanceId != 0)
            {
                CardInstance parent = FindCard(card.ParentCardInstanceId);
                parent.AttachedEnergy.Remove(card);
                parent.AttachedTools.Remove(card);
                parent.AttachedPokemon.Remove(card);

                card.ParentCardInstanceId = 0;
            }
            else
            {
                // Remove from current location's state
                PlayerState player = GetPlayer(card.IsOwnedByPlayer1);
                switch (card.Zone)
                {
                    case Zone.Active:
                        player.Active = null;
                        break;
                    case Zone.Deck:
                        player.Deck.Remove(card);
                        break;
                    case Zone.Hand:
                        player.Hand.Remove(card);
                        break;
                    case Zone.Discard:
                        player.Discard.Remove(card);
                        break;
                    case Zone.Bench:
                        player.Bench.Remove(card);
                        break;
                    case Zone.Prize:
                        player.Prize.Remove(card);
                        break;
                }
            }
        }

        public void MoveCardToBottomOfDeck(CardInstance card)
        {
            if (card.Zone != Zone.Deck)
            {
                throw new InvalidOperationException($"Card is not in a deck");
            }

            PlayerState player = GetPlayer(card.IsOwnedByPlayer1);
            for (int i = 0; i < player.Deck.Count; i++)
            {
                if (player.Deck[i].Id == card.Id)
                {
                    player.Deck.RemoveAt(i);
                    player.Deck.Insert(0, card);
                    return;
                }
            }
            
            throw new InvalidOperationException($"Card not found in deck");
        }

        public void MoveCard(CardInstance card, Zone zone)
        {
            RemoveCardFromCurrentLocation(card);
            
            PlayerState player = GetPlayer(card.IsOwnedByPlayer1);
            // Block attempt to move into occupied active position
            if (zone == Zone.Active && player.Active != null)
            {
                throw new ArgumentException("Attempted to move card to occupied active zone");
            }
            
            // Bump stadium
            if (zone == Zone.Stadium && Stadium != null)
            {
                MoveCard(Stadium, Zone.Discard);
            }
            
            Board.AddCardInstanceToZone(this, card, player, zone);

            // Handle attachments
            if (zone != Zone.Bench && zone != Zone.Active)
            {
                // Attachments don't stay attached, move them separately
                // Need to use a copy, since moving the attachment will modify the collection
                foreach (CardInstance attachment in OnlyAttachments(card).ToList())
                {
                    MoveCard(attachment, zone);
                }
            }
            else
            {
                // Attachments can come along, update their zone
                foreach (CardInstance attachment in OnlyAttachments(card))
                {
                    // Update attachments' zones to match
                    attachment.Zone = zone; 
                }
            }
            
            // Clear damage for cards not in play
            if (zone != Zone.Active && zone != Zone.Bench)
            {
                card.DamageCounters = 0;
            }
        }

        public void AttachEnergy(CardInstance energy, CardInstance target)
        {
            RemoveCardFromCurrentLocation(energy);
            
            energy.ParentCardInstanceId = target.Id;
            energy.Zone = target.Zone;
            
            target.AttachedEnergy.Add(energy);
        }
    }   
}
