namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class AttackUsed : GameEvent
    {
        public bool IsPlayer1;
        public int AttackerId;
        public int TargetId;
        public int AttackIndex;
    }
}