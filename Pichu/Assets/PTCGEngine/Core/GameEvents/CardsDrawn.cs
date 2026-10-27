namespace tylrpage.Pichu.PTCGEngine.Core
{
    // Drawn from deck, hidden info to other player
    public class CardDrawn : GameEvent
    {
        public bool IsPlayer1;
        public int CardId;
    }
}