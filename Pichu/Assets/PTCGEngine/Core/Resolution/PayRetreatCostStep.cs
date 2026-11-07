using System.Collections.Generic;
using System.Linq;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class PayRetreatCostStep : GameStep
    {
        public bool IsPlayer1;
        public List<int> EnergyIds;
        
        public override void Run(ResolverContext ctx)
        {
            List<GameStep> steps = new();
            
            foreach (int energyId in EnergyIds)
            {
                // Move cards first
                steps.Add(new MoveCardStep()
                {
                    CardId = energyId,
                    To = Zone.Discard,
                });
            }
            
            steps.Add(new RequestChoiceRetreatPromoteStep()
            {
                IsPlayer1 = IsPlayer1,
            });
            
            ctx.QueueNext(steps);
        }
    }
}