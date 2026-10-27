using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class EndSetupStep : GameStep
    {
        public override void Run(ResolverContext ctx)
        {
            ctx.WorkingState.Phase = GamePhase.Main;
            
            ctx.Events.Add(new GameStarted());
        }
    }
}