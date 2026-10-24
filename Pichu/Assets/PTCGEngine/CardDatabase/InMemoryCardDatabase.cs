using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.CardDatabase
{
    public class InMemoryCardDatabase : ICardDatabase
    {
        private Dictionary<string, CardDefinition> _memoryDatabase = new();
        
        public CardDefinition GetCardDefinition(string cardDefinitionId)
        {
            if (!_memoryDatabase.TryGetValue(cardDefinitionId, out CardDefinition cardDefinition))
            {
                throw new ArgumentException("Card definition not found, cardDefinitionId: {cardDefinitionId}");
            }
            
            return cardDefinition;
        }

        public bool TryGetCardDefinition(string cardDefinitionId, out CardDefinition cardDefinition)
        {
            return _memoryDatabase.TryGetValue(cardDefinitionId, out cardDefinition);
        }

        public void AddDefinition(CardDefinition cardDefinition)
        {
            _memoryDatabase.Add(cardDefinition.CardDefinitionId, cardDefinition);
        }
    }
}