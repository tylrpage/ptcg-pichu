using System;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ApplyChoiceActiveStep : ApplyChoiceStep
    {
        public override void Run(ResolverContext ctx)
        {
            int activeId = ChosenIds[0];
            ctx.QueueNext(new MoveCardStep()
            {
                CardId = activeId,
                To = Zone.Active,
            });
        }
    }
}