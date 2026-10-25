namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class EndTurnStep : GameStep
    {
        public override void Run(ResolveContext ctx)
        {
            ctx.WorkingState.EndTurnRequested = true;
            
            ctx.Events.Add(new TurnEnded()
            {
                IsPlayer1 = ctx.WorkingState.IsPlayer1Turn,
            });
        }
    }
}