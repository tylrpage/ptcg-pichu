using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class RetreatStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);

            Dictionary<Element, int> energyCounts =
                CardQueries.GetAttachedEnergyCounts(player.Active, ctx.WorkingState, ctx.CardDatabase);
            List<Element> cost = CardQueries.GetRetreatCost(player.Active, ctx.WorkingState, ctx.CardDatabase);

            ctx.WorkingState.Turn.Retreated = true;
            if (energyCounts.Values.Sum() == cost.Count)
            {
                // Active has exactly enough energy, skip the choice and pay all energy
                ctx.QueueNext(new PayRetreatCostStep()
                {
                    IsPlayer1 = IsPlayer1,
                    EnergyIds = player.Active.AttachedEnergy.Select(x => x.Id).ToList(),
                });
            }
            else if (cost.Count == 0)
            {
                // Retreat is free, skip choice and pay nothing
                ctx.QueueNext(new PayRetreatCostStep()
                {
                    IsPlayer1 = IsPlayer1,
                    EnergyIds = new(),
                });
            }
            else
            {
                // Ask player which energies they want to pay
                ctx.WorkingState.PendingChoice = new ChoiceRequest()
                {
                    IsPlayer1 = IsPlayer1,
                    Id = ctx.WorkingState.GetNextChoiceRequest(),
                    Min = cost.Count,
                    Max = cost.Count,
                    Prompt = "Select energies to pay retreat",
                    CandidateIds = player.Active.AttachedEnergy.Select(x => x.Id).ToList(),
                };
                
                ctx.QueueNext(new ApplyChoicePayRetreatCostStep()
                {
                    IsPlayer1 = IsPlayer1,
                    ChoiceId = ctx.WorkingState.PendingChoice.Id,
                });
            }
        }
    }
}