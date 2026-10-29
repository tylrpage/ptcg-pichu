using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class Mulligan : GameEvent
    {
        public bool IsPlayer1;
        public List<int> RevealedCardIds;
    }
}