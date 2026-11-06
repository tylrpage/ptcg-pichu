namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class DamageDealt : GameEvent
    {
        public int AttackerId;
        public int TargetId;
        public int Base;
        public int AfterWeakness;
        public int AfterResistance;
        public int Final;
        public int CountersPlaced;
    }
}