using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class RequestChoiceStartingActiveStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            List<int> basicIds = ctx.WorkingState.GetPlayer(IsPlayer1).Hand
                .Where(x => CardQueries.IsBasicPokemon(x, ctx.WorkingState, ctx.CardDatabase))
                .Select(x => x.Id)
                .ToList();
            ctx.WorkingState.PendingChoice = new ChoiceRequest()
            {
                IsPlayer1 = IsPlayer1,
                Id = ctx.WorkingState.GetNextChoiceRequest(),
                Min = 1,
                Max = 1,
                Prompt = "Choose starting active",
                CandidateIds = basicIds,
            };
            
            ctx.QueueNext(new ApplyChoiceActiveStep()
            {
                ChoiceId = ctx.WorkingState.PendingChoice.Id,
            });
        }
    }
}