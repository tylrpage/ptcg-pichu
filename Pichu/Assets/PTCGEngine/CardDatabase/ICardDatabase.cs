using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.CardDatabase
{
    public interface ICardDatabase
    {
        CardDefinition GetCardDefinition(string cardDefinitionId);
        bool TryGetCardDefinition(string cardDefinitionId, out CardDefinition cardDefinition);
    }
}