using System;
using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class CardInstance
    {
        public int Id;
        public string CardDefinitionId;
        public List<CardInstance> AttachedEnergy = new();
        public List<CardInstance> AttachedTools = new();
        public List<CardInstance> AttachedPokemon = new();
        public bool IsOwnedByPlayer1;
        public int ParentCardInstanceId;
        public Zone Zone;
        public int DamageCounters;
    }   
}
