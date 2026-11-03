using System;
using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

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

            if (gameState.Player1State.Deck.All(x => !CardQueries.IsBasicPokemon(x, gameState, db)))
            {
                throw new ArgumentException($"Deck1 is invalid, has no basics");
            }
            if (gameState.Player2State.Deck.All(x => !CardQueries.IsBasicPokemon(x, gameState, db)))
            {
                throw new ArgumentException($"Deck2 is invalid, has no basics");
            }

            return gameState;
        }

        public struct BattleState
        {
            public string ActiveId;
            public List<string> BenchId;
            public List<string> ActiveEnergyIds;
            public int ActiveDamageCounters;
        }
        // Should only be used for testing purposes, not real games
        public static GameState SetupTestBattle(ICardDatabase db, BattleState player1, BattleState player2)
        {
            List<string> fillerDeck = Enumerable.Repeat(player1.ActiveId, 60).ToList();
            GameState state = Create(db, fillerDeck, fillerDeck);

            for (int i = 0; i < 1; i++)
            {
                bool isPlayer1 = i == 0;
                BattleState battleState = isPlayer1 ? player1 : player2;
                PlayerState player = state.GetPlayer(isPlayer1);
                
                CardInstance active = CreateCardInstance(state, db, battleState.ActiveId, isPlayer1);
                active.DamageCounters = battleState.ActiveDamageCounters;
                Board.AddCardInstanceToZone(state, active, player, Zone.Active);

                foreach (string energyId in battleState.ActiveEnergyIds)
                {
                    CardInstance energy = CreateCardInstance(state, db, energyId, isPlayer1);
                    state.AttachEnergy(energy, active);
                }

                foreach (string benchId in battleState.BenchId)
                {
                    CardInstance card = CreateCardInstance(state, db, benchId, isPlayer1);
                    Board.AddCardInstanceToZone(state, card, player, Zone.Bench);
                }
            }
            
            return state;
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