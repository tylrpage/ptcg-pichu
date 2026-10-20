using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class GameStateFactory
    {
        public GameState GameState;
        
        public void Create(ICardDatabase db, IList<string> deck1, IList<string> deck2)
        {
            // Load up decks
            foreach (string cardDefinitionId in deck1)
            {
                CardInstance cardInstance = CreateCardInstance(cardDefinitionId);
                AddCardInstanceToZone(cardInstance, GameState.Player1State, Zone.Deck);
            }
            foreach (string cardDefinitionId in deck2)
            {
                CardInstance cardInstance = CreateCardInstance(cardDefinitionId);
                AddCardInstanceToZone(cardInstance, GameState.Player2State, Zone.Deck);
            }
        }

        private CardInstance CreateCardInstance(string cardDefinitionId)
        {
            return new CardInstance()
            {
                Id = GameState.GetNextCardInstanceId(),
                CardDefinitionId = cardDefinitionId,
                Zone = Zone.Deck,
            };
        }

        // The one place cards can get a new zone assigned, ensures their Zone prop is kept in sync
        public void AddCardInstanceToZone(CardInstance card, PlayerState playerState, Zone zone)
        {
            switch (zone)
            {
                case Zone.Deck:
                    card.Zone = Zone.Deck;
                    playerState.Deck.Add(card);
                    break;
                case Zone.Bench:
                    card.Zone = Zone.Bench;
                    playerState.Bench.Add(card);
                    break;
                case Zone.Prize:
                    card.Zone = Zone.Prize;
                    playerState.Prize.Add(card);
                    break;
                case Zone.Discard:
                    card.Zone = Zone.Discard;
                    playerState.Discard.Add(card);
                    break;
                case Zone.Hand:
                    card.Zone = Zone.Hand;
                    playerState.Hand.Add(card);
                    break;
                default:
                    throw new ArgumentException("Unknown zone");
            }
        }
    }
}