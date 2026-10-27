using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class DrawStep : GameStep
    {
        public bool IsPlayer1;
        public bool IsTurnDraw;
        // How many cards to draw
        public int Number = 1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            List<CardInstance> deck = player.Deck;
            
            for (int i = 0; i < Number; i++)
            {
                // Check for deckout
                if (deck.Count == 0)
                {
                    // Only an empty deck during draw *for turn* causes a loss
                    if (IsTurnDraw)
                    {
                        // Note: Never set the game as over in a step, have the resolver figure out the final outcome
                        player.FailedToDrawCard = true;    
                    }
                    
                    // Nothing to draw still cancels the drawing regardless 
                    break;
                }

                CardInstance topCard = deck[^1];
                ctx.WorkingState.MoveCard(topCard, Zone.Hand);
                ctx.Events.Add(new CardDrawn()
                {
                    IsPlayer1 = IsPlayer1,
                    CardId = topCard.Id,
                });
            }
        }
    }
}