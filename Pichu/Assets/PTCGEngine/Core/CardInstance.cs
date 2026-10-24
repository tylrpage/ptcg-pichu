using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.CardDatabase;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class CardInstance
    {
        // No card instance should have an id of 0
        public int Id;
        public string CardDefinitionId;
        public List<CardInstance> AttachedEnergy = new();
        public List<CardInstance> AttachedTools = new();
        public List<CardInstance> AttachedPokemon = new();
        public bool IsOwnedByPlayer1;
        // 0 means no parent
        public int ParentCardInstanceId;
        // If has parent, should match parent's zone
        public Zone Zone;
        public int DamageCounters;
    }   
}
