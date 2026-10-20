using System.Collections.Generic;
using Newtonsoft.Json;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class GameState
    {
        public PlayerState Player1State;
        public PlayerState Player2State;
        public CardInstance Stadium;
        public int NextCardInstanceId;
        // public EngineStatus Status;
        
    }   
}
