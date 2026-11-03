using System.Collections.Generic;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.Tests.Tests
{
    public class SimpleBattleTests
    {
        private InMemoryCardDatabase _db;
        
        [SetUp]
        public void Setup()
        {
            _db = TestCards.CreateDatabase();
        }

        [Test]
        public void TestBattle_Works()
        {
            GameState state = GameStateFactory.SetupTestBattle(_db, new GameStateFactory.BattleState()
                {
                    ActiveId = TestCards.TreeckoId,
                    BenchId = new List<string>() { TestCards.TreeckoId, TestCards.TreeckoId },
                    ActiveDamageCounters = 3,
                    ActiveEnergyIds = new List<string>() { TestCards.GrassId }
                },
                new GameStateFactory.BattleState()
                {
                    ActiveId = TestCards.MudkipId,
                    BenchId = new List<string>() { TestCards.MudkipId, TestCards.MudkipId },
                    ActiveDamageCounters = 3,
                    ActiveEnergyIds = new List<string>() { TestCards.WaterId }
                });
        }
    }
}
