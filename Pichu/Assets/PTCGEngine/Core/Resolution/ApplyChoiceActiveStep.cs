using System;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ApplyChoiceActiveStep : GameStep
    {
        public int ChoiceId;
        
        public override void Run(ResolverContext ctx)
        {
            if (ctx.WorkingState.PendingChoice == null)
            {
                throw new NullReferenceException("No pending choice on state");
            }
            ctx.WorkingState.PendingChoice = null;
            
            if (ctx.WorkingState.LastChoiceResult.Id != ChoiceId)
            {
                throw new ArgumentException(
                    $"Wrong choice id, expected {ChoiceId}, got {ctx.WorkingState.LastChoiceResult.Id}");
            }

            int activeId = ctx.WorkingState.LastChoiceResult.ChosenIds[0];
            ctx.QueueNext(new MoveCardStep()
            {
                CardId = activeId,
                To = Zone.Active,
            });
        }
    }
}