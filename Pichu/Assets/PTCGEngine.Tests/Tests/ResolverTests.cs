using Newtonsoft.Json;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;

namespace tylrpage.Pichu.PTCGEngine.Tests.Tests
{
    public class ResolverTests
    {
        private InMemoryCardDatabase _db;
        private CommandResult _setupResult;
        
        [SetUp]
        public void Setup()
        {
            _db = TestCards.CreateDatabase();
            GameState state = GameStateFactory.Create(_db, TestCards.Deck(40, 20), TestCards.Deck(40, 20));
            _setupResult = Resolver.StartGame(state, 0, _db);
        }

        private void AssertGameStateEquality(GameState a, GameState b)
        {
            Assert.That(JsonConvert.SerializeObject(a), Is.EqualTo(JsonConvert.SerializeObject(b)));
        }

        [Test]
        public void Setup_Successful()
        {
            Assert.That(_setupResult.Status, Is.EqualTo(CommandResult.CommandStatus.Ok));
        }

        [Test]
        public void Resolve_UnchangedStateAfterRejection()
        {
            PlayerCommand invalidCommand = PlayerCommand.PlaceBasic(true, 0);
            CommandResult result = Resolver.Submit(_setupResult.State, invalidCommand, _db);
            
            // Check that an invalid command left the state the same
            AssertGameStateEquality(_setupResult.State, result.State);
        }
    }
}