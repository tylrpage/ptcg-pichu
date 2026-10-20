using System;
using System.Collections.Generic;

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

        public Element Type;

        public PokemonStage Stage;
        public CardFormat Format;
        // public List<Action> Attacks = new();
        // public List<Action> Abilities = new();
    }   
}
