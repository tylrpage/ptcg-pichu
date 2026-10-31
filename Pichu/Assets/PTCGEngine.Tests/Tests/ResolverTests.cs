using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using tylrpage.Pichu.PTCGEngine.Random;

namespace tylrpage.Pichu.PTCGEngine.Tests.Tests
{
    public class ResolverTests
    {
        private InMemoryCardDatabase _db;
        private GameState _state;
        
        [SetUp]
        public void Setup()
        {
            _db = TestCards.CreateDatabase();
            _state = GameStateFactory.Create(_db, TestCards.Deck(40, 20), TestCards.Deck(40, 20));
        }

        private CommandResult ChooseActives(GameState state)
        {
            GameState newState = Resolver.Submit(state,
                PlayerCommand.Choose(true, state.PendingChoice.Id, new[] { state.PendingChoice.CandidateIds[0] }),
                _db).State;
            return Resolver.Submit(newState,
                PlayerCommand.Choose(false, newState.PendingChoice.Id, new[] { newState.PendingChoice.CandidateIds[0] }),
                _db);
        }

        private void AssertGameStateEquality(GameState a, GameState b)
        {
            Assert.That(JsonConvert.SerializeObject(a), Is.EqualTo(JsonConvert.SerializeObject(b)));
        }

        [Test]
        public void Setup_Successful()
        {
            CommandResult result = Resolver.StartGame(_state, 0, _db);
            CommandResult chooseActivesResult = ChooseActives(result.State);
            Assert.That(chooseActivesResult.Status, Is.EqualTo(CommandResult.CommandStatus.Ok));
        }

        [Test]
        public void Resolve_UnchangedStateAfterRejection()
        {
            CommandResult setupResult = Resolver.StartGame(_state, 0, _db);
            GameState state = ChooseActives(setupResult.State).State;
            
            // No card has the id 0, so this command should never work
            PlayerCommand invalidCommand = PlayerCommand.PlaceBasic(state.IsPlayer1Turn, 0);
            CommandResult result = Resolver.Submit(state, invalidCommand, _db);

            Assert.That(result.RejectReason, Is.EqualTo(RuleResult.CardNotFound));
            // Check that an invalid command left the state the same
            AssertGameStateEquality(state, result.State);
        }

        [Test]
        public void Resolve_InputStateUnchanged()
        {
            CommandResult setupResult = Resolver.StartGame(_state, 0, _db);
            GameState state = ChooseActives(setupResult.State).State;
            
            PlayerCommand endTurn = PlayerCommand.EndTurn(state.IsPlayer1Turn);
            GameState inputState = state.Clone();
            CommandResult result = Resolver.Submit(state, endTurn, _db);
            
            Assert.That(result.Status, Is.EqualTo(CommandResult.CommandStatus.Ok));
            AssertGameStateEquality(inputState, state);
        }

        [Test]
        public void Setup_Deterministic()
        {
            CommandResult setupA = Resolver.StartGame(_state, 0, _db);
            CommandResult setupB = Resolver.StartGame(_state, 0, _db);
            
            AssertGameStateEquality(setupA.State, setupB.State);
        }

        [Test]
        public void Command_FuzzTest()
        {
            CommandResult setupResult = Resolver.StartGame(_state, 0, _db);
            GameState state = setupResult.State;
            state = ChooseActives(state).State;
            TinyRNG rng = new TinyRNG(0);

            int maxTurns = 5000;
            for (int i = 0; i < maxTurns; i++)
            {
                if (state.Phase == GamePhase.GameOver)
                {
                    TestContext.WriteLine($"Fuzz test ended after {i} loops");
                    break;
                }
                
                List<PlayerCommand> legalCommands = RuleChecker.GetLegalCommands(state, state.IsPlayer1Turn, _db).ToList();
                int randomIndex = rng.NextInt(legalCommands.Count);
                PlayerCommand randomCommand = legalCommands[randomIndex];
                CommandResult result = Resolver.Submit(state, randomCommand, _db);
                state = result.State;
                
                // Make sure legal commands really did only give us legal commands
                Assert.That(result.Status, Is.Not.EqualTo(CommandResult.CommandStatus.Rejected), 
                    result.RejectReason.ToString);
                // Make sure all 120 cards are always accounted for
                Assert.That(result.State.GetAllCards().Count(), Is.EqualTo(120));
            }
            
            Assert.That(state.Phase, Is.EqualTo(GamePhase.GameOver));
        }

        [Test]
        public void Command_CanAttachToActive()
        {
            GameState state = Resolver.StartGame(_state, seed: 0, _db).State;
            state = ChooseActives(state).State;
            PlayerState player = state.GetCurrentTurnPlayer();
            CardInstance energy = player.Hand.First(c => c.CardDefinitionId == TestCards.GrassId);
            int myActive = player.Active.Id;

            CommandResult result = Resolver.Submit(state, PlayerCommand.AttachEnergy(state.IsPlayer1Turn, energy.Id, myActive), _db);
            Assert.That(result.Status, Is.EqualTo(CommandResult.CommandStatus.Ok));
            Assert.That(result.State.GetCurrentTurnPlayer().Active.AttachedEnergy[0].Id, Is.EqualTo(energy.Id));
        }
        
        [Test]
        public void Command_CannotAttachToOpponent()
        {
            GameState state = Resolver.StartGame(_state, seed: 0, _db).State;
            state = ChooseActives(state).State;
            PlayerState player = state.GetCurrentTurnPlayer();
            PlayerState oppPlayer = state.GetPlayer(!state.IsPlayer1Turn);
            CardInstance energy = player.Hand.First(c => c.CardDefinitionId == TestCards.GrassId);
            int opponentActive = oppPlayer.Active.Id;

            CommandResult result = Resolver.Submit(state, PlayerCommand.AttachEnergy(state.IsPlayer1Turn, energy.Id, opponentActive), _db);
            Assert.That(result.RejectReason, Is.EqualTo(RuleResult.TargetNotOwned));
            Assert.That(oppPlayer.Active.AttachedEnergy.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void Command_CannotAttachEnergyTwice()
        {
            GameState state = Resolver.StartGame(_state, seed: 0, _db).State;
            state = ChooseActives(state).State;
            PlayerState player = state.GetCurrentTurnPlayer();
            List<CardInstance> energies = player.Hand.Where(c => c.CardDefinitionId == TestCards.GrassId)
                .ToList();
            int myActive = player.Active.Id;
            
            Assert.That(energies.Count, Is.GreaterThanOrEqualTo(2));

            CommandResult result = Resolver.Submit(state, PlayerCommand.AttachEnergy(state.IsPlayer1Turn, energies[0].Id, myActive), _db);
            Assert.That(result.Status, Is.EqualTo(CommandResult.CommandStatus.Ok));
            state = result.State;
            
            // Try attaching again
            result = Resolver.Submit(state, PlayerCommand.AttachEnergy(state.IsPlayer1Turn, energies[1].Id, myActive), _db);
            Assert.That(result.RejectReason, Is.EqualTo(RuleResult.EnergyAlreadyAttachedForTurn));
        }
        
        [Test]
        public void Command_CannotBenchEnergy()
        {
            GameState state = Resolver.StartGame(_state, seed: 0, _db).State;
            state = ChooseActives(state).State;
            PlayerState player = state.GetCurrentTurnPlayer();
            CardInstance energy = player.Hand.First(c => c.CardDefinitionId == TestCards.GrassId);

            CommandResult result = Resolver.Submit(state, PlayerCommand.PlaceBasic(state.IsPlayer1Turn, energy.Id), _db);
            Assert.That(result.RejectReason, Is.EqualTo(RuleResult.NotBasicPokemon));
        }
        
        [Test]
        public void Command_CannotAttachDuringOpponentsTurn()
        {
            GameState state = Resolver.StartGame(_state, seed: 0, _db).State;
            state = ChooseActives(state).State;
            PlayerState player = state.GetCurrentTurnPlayer();
            CardInstance energy = player.Hand.First(c => c.CardDefinitionId == TestCards.GrassId);
            int myActive = player.Active.Id;

            CommandResult result = Resolver.Submit(state, PlayerCommand.AttachEnergy(!state.IsPlayer1Turn, energy.Id, myActive), _db);
            Assert.That(result.RejectReason, Is.EqualTo(RuleResult.NotYourTurn));
        }
    }
}