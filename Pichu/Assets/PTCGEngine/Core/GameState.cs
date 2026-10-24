using System.Collections.Generic;
using Newtonsoft.Json;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class GameState
    {
        public PlayerState Player1State;
        public PlayerState Player2State;
        public CardInstance Stadium;
        public int NextCardInstanceId;
        // public EngineStatus Status;
        
        private static readonly JsonSerializerSettings CloneSettings = new()
        {
            // Creates fresh lists for fields, for lists that are pre-filled at init
            ObjectCreationHandling = ObjectCreationHandling.Replace,
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
        
        public GameState Clone()
        {
            // Serializes public fields, fields with [JsonProperty], and ignores fields with [JsonIgnore]
            string json = JsonConvert.SerializeObject(this, CloneSettings);
            return JsonConvert.DeserializeObject<GameState>(json, CloneSettings);
        }

        public int GetNextCardInstanceId()
        {
            return ++NextCardInstanceId;
        }
    }   
}
