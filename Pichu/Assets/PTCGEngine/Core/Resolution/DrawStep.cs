using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class DrawStep : GameStep
    {
        public bool IsPlayer1;
        public bool IsTurnDraw;
        
        public override void Run(ResolveContext ctx)
        {
            // Check for deckout
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            List<CardInstance> deck = player.Deck;
            if (deck.Count == 0)
            {
                // Only an empty deck during draw *for turn* causes a loss
                if (IsTurnDraw)
                {
                    // Note: Never set the game as over in a step, have the resolver figure out the final outcome
                    player.FailedToDrawCard = true;    
                }
                return;
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