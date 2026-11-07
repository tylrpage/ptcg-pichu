using System.Linq;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class RequestChoiceRetreatPromoteStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            // Ask which benched mon to replace with
            ctx.WorkingState.PendingChoice = new ChoiceRequest()
            {
                IsPlayer1 = IsPlayer1,
                Id = ctx.WorkingState.GetNextChoiceRequest(),
                Min = 1,
                Max = 1,
                Prompt = "Choose pokemon to promote",
                CandidateIds = player.Bench.Select(x => x.Id).ToList(),
            };

            ctx.QueueNext(new ApplyChoiceRetreatAndPromoteStep()
            {
                ChoiceId = ctx.WorkingState.PendingChoice.Id,
                IsPlayer1 = IsPlayer1,
            });
        }
    }
}