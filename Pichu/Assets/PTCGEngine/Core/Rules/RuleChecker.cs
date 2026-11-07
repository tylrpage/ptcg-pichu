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
        ChoicePending,
        ChoiceIdMismatch,
        ChoiceWrongPlayer,
        InvalidChoiceIds,
        InvalidChoiceCount,
        IndistinctChoiceIds,
        AttackerNotInActive,
        InvalidIndex,
        CannotAffordCost,
        CannotAttackOnFirstGameTurn,
        RetreatAlreadyUsedForTurn,
        BenchEmpty,
    }
    
    public static class RuleChecker
    {
        public static List<PlayerCommand> GetLegalCommands(GameState state, bool isPlayer1, ICardDatabase db)
        {
            List<PlayerCommand> possibleCommands = new();
            // The UI will use a different function to get legal choose command options
            if (state.PendingChoice != null)
            {
                return possibleCommands;
            }
            
            PlayerState player = state.GetPlayer(isPlayer1);
            
            // End turn
            possibleCommands.Add(PlayerCommand.EndTurn(isPlayer1));
            
            // Retreat
            possibleCommands.Add(PlayerCommand.Retreat(isPlayer1));
            
            // Place basic
            // Generate for every pokemon in hand
            possibleCommands.AddRange(player.Hand
                .Where(x => CardQueries.IsPokemon(x, state, db))
                .Select(x => PlayerCommand.PlaceBasic(isPlayer1, x.Id)));
            
            // Generate for every energy and in play pokemon pair
            IEnumerable<CardInstance> energiesInHand = player.Hand.Where(x => CardQueries.IsEnergy(x, state, db));
            IEnumerable<CardInstance> pokemonInPlay =
                // filter out null because Active could be null before setup is done
                player.Bench.Append(player.Active).Where(x => x != null && CardQueries.IsPokemon(x, state, db));
            foreach (CardInstance energy in energiesInHand)
            {
                foreach (CardInstance pokemon in pokemonInPlay)
                {
                    possibleCommands.Add(PlayerCommand.AttachEnergy(isPlayer1, energy.Id, pokemon.Id));
                }
            }
            
            // Generate attack commands for each attack on active
            for (int i = 0; i < CardQueries.GetAttacks(player.Active, state, db).Count; i++)
            {
                possibleCommands.Add(PlayerCommand.Attack(isPlayer1, player.Active.Id, i));
            }
            
            // Filter for legal commands
            return possibleCommands.Where(x => Check(x, state, db) == RuleResult.Ok).ToList();
        }
        
        public static RuleResult Check(PlayerCommand command, GameState state, ICardDatabase db)
        {
            if (state.Player1Won.HasValue)
            {
                return RuleResult.GameAlreadyOver;
            }

            // Choose is the only command that is allowed to come in at any player's turn
            // b/c we might need input from player 2 on player 1's turn for example
            if (command.Type != CommandType.Choose && state.IsPlayer1Turn != command.IsPlayer1)
            {
                return RuleResult.NotYourTurn;
            }
            
            if (state.Phase != GamePhase.Main &&
                // Allow choose command during setup
                !(command.Type == CommandType.Choose && state.Phase == GamePhase.Setup))
            {
                return RuleResult.WrongPhase;
            }

            if (state.PendingChoice != null && command.Type != CommandType.Choose)
            {
                return RuleResult.ChoicePending;
            }

            switch (command.Type)
            {
                case CommandType.None:
                    return RuleResult.NoCommand;
                case CommandType.PlaceBasic:
                    return CheckPlaceBasic(state, command, db);
                case CommandType.ManualAttachEnergy:
                    return CheckAttachEnergy(state, command, db);
                case CommandType.Choose:
                    return CheckChoose(state, command, db);
                case CommandType.Attack:
                    return CheckAttack(state, command, db);
                case CommandType.Retreat:
                    return CheckRetreat(state, command, db);
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

            if (!CardQueries.IsPokemon(target, state, db))
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

        private static RuleResult CheckChoose(GameState state, PlayerCommand command, ICardDatabase db)
        {
            if (state.PendingChoice == null || state.PendingChoice.Id != command.ChoiceRequestId)
            {
                return RuleResult.ChoiceIdMismatch;
            }

            if (state.PendingChoice.IsPlayer1 != command.IsPlayer1)
            {
                return RuleResult.ChoiceWrongPlayer;
            }

            if (command.ChosenIds == null)
            {
                return RuleResult.InvalidChoiceIds; 
            }

            if (command.ChosenIds.Distinct().Count() != command.ChosenIds.Length)
            {
                return RuleResult.IndistinctChoiceIds;
            }

            if (command.ChosenIds.Any(x => !state.PendingChoice.CandidateIds.Contains(x)))
            {
                return RuleResult.InvalidChoiceIds;
            }

            if (command.ChosenIds.Length > state.PendingChoice.Max ||
                command.ChosenIds.Length < state.PendingChoice.Min)
            {
                return RuleResult.InvalidChoiceCount;
            }

            return RuleResult.Ok;
        }

        private static RuleResult CheckAttack(GameState state, PlayerCommand command, ICardDatabase db)
        {
            if (state.IsPlayer1Turn != command.IsPlayer1)
            {
                return RuleResult.NotYourTurn;
            }

            if (CardQueries.IsFirstTurnOfPlayerGoingFirst(state))
            {
                return RuleResult.CannotAttackOnFirstGameTurn;
            }

            CardInstance attacker = state.FindCard(command.CardId);
            if (attacker == null)
            {
                return RuleResult.CardNotFound;
            }

            if (attacker.IsOwnedByPlayer1 != command.IsPlayer1)
            {
                return RuleResult.NotOwned;
            }

            if (attacker.Zone != Zone.Active)
            {
                return RuleResult.AttackerNotInActive;
            }

            CardDefinition attackerDefinition = db.GetCardDefinition(attacker.CardDefinitionId);
            if (command.ActionIndex < 0 || command.ActionIndex >= attackerDefinition.Attacks.Count)
            {
                return RuleResult.InvalidIndex;
            }
            Attack attack = attackerDefinition.Attacks[command.ActionIndex];

            if (!CardQueries.CanPayCost(attack.Cost, attacker, state, db))
            {
                return RuleResult.CannotAffordCost;
            }

            return RuleResult.Ok;
        }

        private static RuleResult CheckRetreat(GameState state, PlayerCommand command, ICardDatabase db)
        {
            if (state.Turn.Retreated)
            {
                return RuleResult.RetreatAlreadyUsedForTurn;
            }

            PlayerState player = state.GetPlayer(command.IsPlayer1);
            if (player.Bench.Count == 0)
            {
                return RuleResult.BenchEmpty;
            }

            CardInstance active = player.Active;
            List<Element> retreatCost = CardQueries.GetRetreatCost(active, state, db);
            if (!CardQueries.CanPayCost(retreatCost, active, state, db))
            {
                return RuleResult.CannotAffordCost;
            }

            return RuleResult.Ok;
        }
    }
}