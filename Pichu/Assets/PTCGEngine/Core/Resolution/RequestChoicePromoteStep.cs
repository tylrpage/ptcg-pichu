using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class RequestChoicePromoteStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            List<int> benchedIds = ctx.WorkingState.GetPlayer(IsPlayer1).Bench
                .Select(x => x.Id)
                .ToList();
            ctx.WorkingState.PendingChoice = new ChoiceRequest()
            {
                IsPlayer1 = IsPlayer1,
                Id = ctx.WorkingState.GetNextChoiceRequest(),
                Min = 1,
                Max = 1,
                Prompt = "Promote new active",
                CandidateIds = benchedIds,
            };
            
            ctx.QueueNext(new ApplyChoiceActiveStep()
            {
                ChoiceId = ctx.WorkingState.PendingChoice.Id,
            });
        }
    }
}