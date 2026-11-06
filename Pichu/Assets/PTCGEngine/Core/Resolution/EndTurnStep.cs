namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class EndTurnStep : GameStep
    {
        public override void Run(ResolverContext ctx)
        {
            ctx.WorkingState.EndTurnRequested = true;
        }
    }
}