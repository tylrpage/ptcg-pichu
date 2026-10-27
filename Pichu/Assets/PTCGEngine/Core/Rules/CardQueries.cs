using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core.Rules
{
    public static class CardQueries
    {
        public const int StartingHandSize = 7;
        public const int StartingPrizeCardCount = 6;
        
        public static bool IsBasicPokemon(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition cardDefinition = db.GetCardDefinition(card.CardDefinitionId);

            // todo: handle fossils
            return IsPokemon(card, state, db) && cardDefinition.Stage == PokemonStage.Basic;
        }

        public static bool IsEnergy(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition cardDefinition = db.GetCardDefinition(card.CardDefinitionId);

            return cardDefinition.Format == CardFormat.Energy || cardDefinition.Format == CardFormat.SpecialEnergy;
        }
        
        public static bool IsPokemon(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition cardDefinition = db.GetCardDefinition(card.CardDefinitionId);

            return cardDefinition.Format == CardFormat.Pokemon;
        }

        public static int GetMaxBenchSize(PlayerState player, GameState state)
        {
            return 5;
        }

        public static int GetMaxHP(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition cardDefinition = db.GetCardDefinition(card.CardDefinitionId);
            return cardDefinition.HP;
        }

        public static int GetRemainingHP(CardInstance card, GameState state, ICardDatabase db)
        {
            int maxHP = GetMaxHP(card, state, db);
            return maxHP - (card.DamageCounters * 10);
        }
    }
}