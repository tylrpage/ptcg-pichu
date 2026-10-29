using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class ChoiceRequest
    {
        public int Id;
        public bool IsPlayer1;
        public List<int> CandidateIds;
        public int Min;
        public int Max;
        public string Prompt;
    }
}