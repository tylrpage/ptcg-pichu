using System.Collections.Generic;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.Tests.Tests
{
    // Contains helper classes to setup tests
    public class TestCards
    {
        public const string TreeckoId = "test_001";
        public const string MudkipId = "test_002";
        public const string GrassId = "test_energy_g";

        public static InMemoryCardDatabase CreateDatabase()
        { 
            InMemoryCardDatabase db = new InMemoryCardDatabase();
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = TreeckoId,
                Name = "Treecko",
                Format = CardFormat.Pokemon,
            });
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = MudkipId,
                Name = "Mudkip",
                Format = CardFormat.Pokemon,
            });
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = GrassId,
                Name = "Grass Energy",
                Format = CardFormat.Energy,
            });

            return db;
        }

        public static List<string> Deck(int pokemon, int energy)
        {
            List<string> deck = new();
            for (int i = 0; i < pokemon; i++)
            {
                deck.Add(TreeckoId);
            }
            for (int i = 0; i < energy; i++)
            {
                deck.Add(GrassId);
            }

            return deck;
        }
    }
}
