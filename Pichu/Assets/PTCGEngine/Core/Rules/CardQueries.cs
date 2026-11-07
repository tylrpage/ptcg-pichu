using System.Collections.Generic;
using System.Linq;
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

        public static bool IsFirstTurnOfPlayerGoingFirst(GameState state)
        {
            return state.TurnNumber == 1;
        }

        // Only supports simple, single type energy for now
        public static Element GetProvidedEnergy(CardInstance energy, ICardDatabase db)
        {
            CardDefinition cardDefinition = db.GetCardDefinition(energy.CardDefinitionId);

            return cardDefinition.Type;
        }

        public static Dictionary<Element, int> GetAttachedEnergyCounts(CardInstance pokemon, GameState state, ICardDatabase db)
        {
            Dictionary<Element, int> energyCounts = new();
            foreach (CardInstance energy in pokemon.AttachedEnergy)
            {
                Element element = GetProvidedEnergy(energy, db);
                energyCounts.TryAdd(element, 0);
                energyCounts[element]++;
            }

            return energyCounts;
        }
        
        // Only supports simple, single type energy for now
        public static bool CanPayCost(List<Element> costs, CardInstance pokemon, GameState state, ICardDatabase db)
        {
            Dictionary<Element, int> energyCounts = GetAttachedEnergyCounts(pokemon, state, db);

            int colorlessNeeded = 0;
            foreach (Element cost in costs)
            {
                if (cost == Element.Colorless)
                {
                    colorlessNeeded++;
                }
                else
                {
                    if (energyCounts.GetValueOrDefault(cost) == 0)
                    {
                        return false;
                    }
                    energyCounts[cost]--;
                }
            }

            return energyCounts.Values.Sum() >= colorlessNeeded;
        }

        public static List<Attack> GetAttacks(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition definition = db.GetCardDefinition(card.CardDefinitionId);
            return definition.Attacks.ToList();
        }

        public static int GetPrizeValue(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition definition = db.GetCardDefinition(card.CardDefinitionId);
            return definition.PrizeValue;
        }

        public static List<Element> GetRetreatCost(CardInstance card, GameState state, ICardDatabase db)
        {
            CardDefinition definition = db.GetCardDefinition(card.CardDefinitionId);
            return Enumerable.Repeat(Element.Colorless, definition.RetreatCost).ToList();
        }
    }
}