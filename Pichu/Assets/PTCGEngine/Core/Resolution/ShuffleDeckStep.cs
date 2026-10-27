using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ShuffleDeckStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            ctx.WorkingState.Rng.Shuffle(player.Deck);
            
            ctx.Events.Add(new DeckShuffled()
            {
                IsPlayer1 = IsPlayer1,
            });
        }
    }
}