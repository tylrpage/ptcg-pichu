using System;
using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core.Rules
{
    public enum RuleResult
    {
        Ok,
        GameAlreadyOver,
        NotYourTurn,
        WrongPhase,
        NoCommand,
        CardNotFound,
        NotInHand,
        NotBasicPokemon,
        BenchFull,
        NotEnergy,
        NotOwned,
        TargetNotOwned,
        NotPokemon,
        TargetNotInPlay,
        TargetIsAttached,
        EnergyAlreadyAttachedForTurn,
    }
    
    public static class RuleChecker
    {
        public static IEnumerable<PlayerCommand> GetLegalCommands(GameState state, bool isPlayer1Turn, ICardDatabase db)
        {
            List<PlayerCommand> possibleCommands = new();
            PlayerState player = state.GetPlayer(isPlayer1Turn);
            
            // End turn
            possibleCommands.Add(PlayerCommand.EndTurn(isPlayer1Turn));
            
            // Place basic
            // Generate for every pokemon in hand
            possibleCommands.AddRange(player.Hand
                .Where(x => CardQueries.IsPokemon(x, state, db))
                .Select(x => PlayerCommand.PlaceBasic(isPlayer1Turn, x.Id)));
            
            // Generate for every energy and in play pokemon pair
            IEnumerable<CardInstance> energiesInHand = player.Hand.Where(x => CardQueries.IsEnergy(x, state, db));
            IEnumerable<CardInstance> pokemonInPlay =
                player.Bench.Append(player.Active).Where(x => CardQueries.IsPokemon(x, state, db));
            foreach (CardInstance energy in energiesInHand)
            {
                foreach (CardInstance pokemon in pokemonInPlay)
                {
                    possibleCommands.Add(PlayerCommand.AttachEnergy(isPlayer1Turn, energy.Id, pokemon.Id));
                }
            }
            
            // Filter for legal commands
            return possibleCommands.Where(x => Check(x, state, db) == RuleResult.Ok);
        }
        
        public static RuleResult Check(PlayerCommand command, GameState state, ICardDatabase db)
        {
            if (state.Player1Won.HasValue)
            {
                return RuleResult.GameAlreadyOver;
            }

            if (state.IsPlayer1Turn != command.IsPlayer1)
            {
                return RuleResult.NotYourTurn;
            }

            if (state.Phase != GamePhase.Main)
            {
                return RuleResult.WrongPhase;
            }

            switch (command.Type)
            {
                case CommandType.None:
                    return RuleResult.NoCommand;
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
                return RuleResult.CardNotFound;
            }
            
            // Card is in hand (if true, we know we own it)
            if (!state.GetPlayer(command.IsPlayer1).Hand.Contains(card))
            {
                return RuleResult.NotInHand;
            }

            // Card is a basic pokemon
            if (!CardQueries.IsBasicPokemon(card, state, db))
            {
                return RuleResult.NotBasicPokemon;
            }

            // Has bench space
            int maxBenchSize = CardQueries.GetMaxBenchSize(state.GetPlayer(command.IsPlayer1), state);
            if (state.GetPlayer(command.IsPlayer1).Bench.Count >= maxBenchSize)
            {
                return RuleResult.BenchFull;
            }

            return RuleResult.Ok;
        }

        private static RuleResult CheckAttachEnergy(GameState state, PlayerCommand command, ICardDatabase db)
        {
            // Energy wasn't already attached for turn
            if (state.Turn.EnergyAttached)
            {
                return RuleResult.EnergyAlreadyAttachedForTurn;
            }
            
            // Check energy
            CardInstance card = state.FindCard(command.CardId);
            if (card == null)
            {
                return RuleResult.CardNotFound;
            }

            if (!CardQueries.IsEnergy(card, state, db))
            {
                return RuleResult.NotEnergy;
            }
            
            if (card.IsOwnedByPlayer1 != command.IsPlayer1)
            {
                return RuleResult.NotOwned;
            }
            
            // Energy is in hand (if true, we know we own it)
            if (!state.GetPlayer(command.IsPlayer1).Hand.Contains(card))
            {
                return RuleResult.NotInHand;
            }
            
            // Check target
            CardInstance target = state.FindCard(command.TargetId);
            if (target == null)
            {
                return RuleResult.CardNotFound;
            }

            if (!CardQueries.IsPokemon(card, state, db))
            {
                return RuleResult.NotPokemon;
            }

            if (target.Zone != Zone.Active && target.Zone != Zone.Bench)
            {
                return RuleResult.TargetNotInPlay;
            }

            if (target.IsOwnedByPlayer1 != command.IsPlayer1)
            {
                return RuleResult.TargetNotOwned;
            }

            if (target.ParentCardInstanceId != 0)
            {
                return RuleResult.TargetIsAttached;
            }

            return RuleResult.Ok;
        }
    }
}