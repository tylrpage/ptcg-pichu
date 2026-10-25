namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class CardMoved : GameEvent
    {
        public int CardId;
        public Zone From;
        public Zone To;
        // Set when we moved because our parent moved
        public int FromParentId;
    }
}