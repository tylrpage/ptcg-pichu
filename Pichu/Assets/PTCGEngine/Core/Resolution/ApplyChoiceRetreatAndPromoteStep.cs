namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ApplyChoiceRetreatAndPromoteStep : ApplyChoiceStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            CardInstance active = ctx.WorkingState.GetPlayer(IsPlayer1).Active;
            CardInstance benchReplacement = ctx.WorkingState.FindCard(ChosenIds[0]);

            ctx.QueueNext(new MoveCardStep()
                {
                    CardId = active.Id,
                    To = Zone.Bench,
                },
                new MoveCardStep()
                {
                    CardId = benchReplacement.Id,
                    To = Zone.Active,
                });
        }
    }
}