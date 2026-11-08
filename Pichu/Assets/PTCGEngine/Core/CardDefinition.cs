using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Effects;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class CardDefinition
    {
        // Follows {setId}_{setNumber} format
        public string CardDefinitionId;
        public string Name;
        public int HP;
        public int RetreatCost;
        public Element? Weakness;
        public Element? Resistance;
        public int PrizeValue = 1;

        public Element Type;

        public PokemonStage Stage;
        public CardFormat Format;
        public List<Attack> Attacks = new();
        public Ability Ability;
    }

    public class Attack
    {
        public string Name;
        public List<Element> Cost;
        public List<CardEffect> Effects;
    }

    public class Ability
    {
        public string Name;
        public TriggerType Trigger;
        public TriggerScope Scope;
        public List<CardEffect> Effects;
    }

    public enum TriggerType
    {
        PlayedToBenchFromHand,
    }

    public enum TriggerScope
    {
        Self,
        Any,
    }
}
