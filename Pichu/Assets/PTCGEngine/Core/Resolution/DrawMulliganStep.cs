using System;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    // todo: have the player choose how many cards of their mulligan they want to draw
    public class DrawMulliganStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            int mulliganDiff = (ctx.WorkingState.Player1State.MulliganCount -
                                ctx.WorkingState.Player2State.MulliganCount) *
                               (IsPlayer1 ? 1 : -1);

            if (mulliganDiff > 0)
            {
                ctx.QueueNext(new DrawStep()
                {
                    IsPlayer1 = IsPlayer1,
                    Number = mulliganDiff,
                    IsTurnDraw = false,
                });
            }
        }
    }
}