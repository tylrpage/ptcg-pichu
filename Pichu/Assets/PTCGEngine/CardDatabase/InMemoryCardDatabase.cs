using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.CardDatabase
{
    public class InMemoryCardDatabase : ICardDatabase
    {
        private Dictionary<string, CardDefinition> _memoryDatabase = new();
        
        public CardDefinition GetDefinition(string cardDefinitionId)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetCardDefinition(string cardDefinitionId, out CardDefinition cardDefinition)
        {
            throw new System.NotImplementedException();
        }
    }
}