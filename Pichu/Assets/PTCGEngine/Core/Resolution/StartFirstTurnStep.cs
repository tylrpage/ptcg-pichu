namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class StartFirstTurnStep : GameStep
    {
        public override void Run(ResolverContext ctx)
        {
            ctx.QueueNext(new StartTurnStep()
            {
                IsPlayer1 = ctx.WorkingState.Player1GoesFirst,
            });
        }
    }
}