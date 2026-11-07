using System.Linq;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ApplyChoicePayRetreatCostStep : ApplyChoiceStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            ctx.QueueNext(new PayRetreatCostStep()
            {
                IsPlayer1 = IsPlayer1,
                EnergyIds = ChosenIds.ToList(),
            });
        }
    }
}