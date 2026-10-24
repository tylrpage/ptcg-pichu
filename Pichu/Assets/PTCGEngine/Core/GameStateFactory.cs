using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public static class GameStateFactory
    {
        public static GameState Create(ICardDatabase db, IList<string> deck1, IList<string> deck2)
        {
            GameState gameState = new GameState()
            {
                Player1State = new(),
                Player2State = new(),
            };
            
            // Load up decks
            foreach (string cardDefinitionId in deck1)
            {
                CardInstance cardInstance = CreateCardInstance(gameState, db, cardDefinitionId, true);
                Board.AddCardInstanceToZone(gameState, cardInstance, gameState.Player1State, Zone.Deck);
            }
            foreach (string cardDefinitionId in deck2)
            {
                CardInstance cardInstance = CreateCardInstance(gameState, db, cardDefinitionId, false);
                Board.AddCardInstanceToZone(gameState, cardInstance, gameState.Player2State, Zone.Deck);
            }

            return gameState;
        }

        private static CardInstance CreateCardInstance(GameState gameState, ICardDatabase db, string cardDefinitionId, bool isOwnedByPlayer1)
        {
            if (!db.TryGetCardDefinition(cardDefinitionId, out CardDefinition cardDefinition))
            {
                throw new ArgumentException($"Unknown card definition id: {cardDefinitionId}");
            }
            
            return new CardInstance()
            {
                Id = gameState.GetNextCardInstanceId(),
                CardDefinitionId = cardDefinitionId,
                Zone = Zone.Deck,
                IsOwnedByPlayer1 = isOwnedByPlayer1,
            };
        }
    }
}