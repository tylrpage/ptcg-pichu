using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    // Draws hand until there is a basic
    public class SetupDrawHandStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            // Shuffle and draw hand
            ctx.QueueNext(new ShuffleDeckStep()
            {
                IsPlayer1 = IsPlayer1,
            },
            new DrawStep()
            {
                IsPlayer1 = IsPlayer1,
                Number = CardQueries.StartingHandSize,
                IsTurnDraw = false,
            },
            new SetupCheckForMulligan()
            {
                IsPlayer1 = IsPlayer1,
            });
        }
    }
}