using System;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core.Rules
{
    public static class RuleChecker
    {
        public enum RuleResult
        {
            Ok,
            Invalid,
        }
        
        public static RuleResult Check(GameState state, PlayerCommand command, ICardDatabase db)
        {
            if (state.Player1Won.HasValue)
            {
                return RuleResult.Invalid;
            }

            if (state.IsPlayer1Turn != command.IsPlayer1)
            {
                return RuleResult.Invalid;
            }

            if (state.Phase != GamePhase.Main)
            {
                return RuleResult.Invalid;
            }

            switch (command.Type)
            {
                case CommandType.None:
                    return RuleResult.Invalid;
                case CommandType.PlaceBasic:
                    return CheckPlaceBasic(state, command, db);
                case CommandType.ManualAttachEnergy:
                    return CheckAttachEnergy(state, command, db);
                case CommandType.EndTurn:
                    return RuleResult.Ok;
                default:
                    throw new NotImplementedException($"Missing check for command type: {command.Type}");
            }
        }

        private static RuleResult CheckPlaceBasic(GameState state, PlayerCommand command, ICardDatabase db)
        {
            CardInstance card = state.FindCard(command.CardId);
            if (card == null)
            {
                return RuleResult.Invalid;
            }
            
            // Card is in hand (if true, we know we own it)
            if (!state.GetPlayer(command.IsPlayer1).Hand.Contains(card))
            {
                return RuleResult.Invalid;
            }

            // Card is a basic pokemon
            if (!CardQueries.IsBasicPokemon(card, state, db))
            {
                return RuleResult.Invalid;
            }

            // Has bench space
            int maxBenchSize = CardQueries.GetMaxBenchSize(state.GetPlayer(command.IsPlayer1), state);
            if (state.GetPlayer(command.IsPlayer1).Bench.Count >= maxBenchSize)
            {
                return RuleResult.Invalid;
            }

            return RuleResult.Ok;
        }

        private static RuleResult CheckAttachEnergy(GameState state, PlayerCommand command, ICardDatabase db)
        {
            // Check energy
            CardInstance card = state.FindCard(command.CardId);
            if (card == null)
            {
                return RuleResult.Invalid;
            }

            if (!CardQueries.IsEnergy(card, state, db))
            {
                return RuleResult.Invalid;
            }
            
            if (card.IsOwnedByPlayer1 != command.IsPlayer1)
            {
                return RuleResult.Invalid;
            }
            
            // Energy is in hand (if true, we know we own it)
            if (!state.GetPlayer(command.IsPlayer1).Hand.Contains(card))
            {
                return RuleResult.Invalid;
            }
            
            // Check target
            CardInstance target = state.FindCard(command.TargetId);
            if (target == null)
            {
                return RuleResult.Invalid;
            }

            if (!CardQueries.IsPokemon(card, state, db))
            {
                return RuleResult.Invalid;
            }

            if (target.Zone != Zone.Active && target.Zone != Zone.Bench)
            {
                return RuleResult.Invalid;
            }

            if (target.IsOwnedByPlayer1 != command.IsPlayer1)
            {
                return RuleResult.Invalid;
            }

            if (target.ParentCardInstanceId != 0)
            {
                return RuleResult.Invalid;
            }
            
            // Energy wasn't already attached for turn
            if (state.Turn.EnergyAttached)
            {
                return RuleResult.Invalid;
            }

            return RuleResult.Ok;
        }
    }
}