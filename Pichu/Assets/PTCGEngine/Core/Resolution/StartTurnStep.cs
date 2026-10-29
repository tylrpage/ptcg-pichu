namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class StartTurnStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            ctx.WorkingState.TurnNumber++;
            ctx.WorkingState.IsPlayer1Turn = IsPlayer1;
            ctx.WorkingState.Turn = new();
            
            ctx.Events.Add(new TurnStarted()
            {
                IsPlayer1 = IsPlayer1,
                TurnNumber = ctx.WorkingState.TurnNumber,
            });
            
            ctx.QueueNext(new DrawStep()
            {
                IsPlayer1 = ctx.WorkingState.IsPlayer1Turn,
                IsTurnDraw = true,
                Number = 1,
            });
        }
    }
}