using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public static class CardInstanceExtensions
    {
        public static CardDefinition GetDefinition(this CardInstance card, ICardDatabase db)
        {
            return db.GetCardDefinition(card.CardDefinitionId);
        }
    }
}