using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class GameSetupStep : GameStep
    {
        public override void Run(ResolverContext ctx)
        {
            // Coin flip
            bool isHeads = ctx.WorkingState.Rng.CoinFlip();
            // todo: for now, the flip winner doesn't choose who goes first
            bool isPlayer1First = isHeads;
            ctx.WorkingState.Player1GoesFirst = isPlayer1First;
            ctx.Events.Add(new CoinFlipped()
            {
                IsHeads = isHeads,
                IsPlayer1First = isPlayer1First,
            });

            // Shuffle and draw hands for each player until they have basic
            ctx.QueueNext(new SetupDrawHandStep()
            {
                IsPlayer1 = true,
            },
            new SetupDrawHandStep()
            {
                IsPlayer1 = false,
            },
            // Ask each player to put basic in active
            new RequestChoiceActiveStep()
            {
                IsPlayer1 = true,
            },
            new RequestChoiceActiveStep()
            {
                IsPlayer1 = false,
            });
        }
    }
}