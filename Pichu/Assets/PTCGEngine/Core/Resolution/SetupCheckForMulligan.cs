using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    // Draws hand until there is a basic
    public class SetupCheckForMulligan : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            bool hasBasicInHand = player.Hand.Any(x => 
                CardQueries.IsBasicPokemon(x, ctx.WorkingState, ctx.CardDatabase));

            List<GameStep> steps = new();
            if (!hasBasicInHand)
            {
                player.MulliganCount++;
                ctx.Events.Add(new Mulligan()
                {
                    IsPlayer1 = IsPlayer1,
                    RevealedCardIds = player.Hand.Select(x => x.Id).ToList(),
                });
                
                // Return hand to deck
                foreach (CardInstance card in player.Hand)
                {
                    steps.Add(new MoveCardStep()
                    {
                        CardId = card.Id,
                        To = Zone.Deck,
                    });
                }
                
                // Draw a new hand
                steps.Add(new SetupDrawHandStep());
            }
            
            ctx.QueueNext(steps);
        }
    }
}