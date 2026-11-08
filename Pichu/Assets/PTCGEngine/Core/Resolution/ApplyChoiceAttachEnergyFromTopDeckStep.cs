using System.Collections.Generic;
using System.Linq;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class ApplyChoiceAttachEnergyFromTopDeckStep : ApplyChoiceStep
    {
        public int CardId;
        public List<int> LookedAtCards;
        
        public override void Run(ResolverContext ctx)
        {
            List<int> remainingCards = LookedAtCards.ToList();
            if (ChosenIds.Length > 0)
            {
                remainingCards.Remove(ChosenIds[0]);
                ctx.QueueNext(new AttachEnergyStep()
                {
                    EnergyId = ChosenIds[0],
                    TargetId = CardId,
                    IsManual = false,
                });
            }
            
            ctx.WorkingState.Rng.Shuffle(remainingCards);
            foreach (int remainingCard in remainingCards)
            {
                ctx.QueueNext(new MoveCardStep()
                {
                    CardId = remainingCard,
                    To = Zone.Deck,
                    ToBottom = true,
                });   
            }
        }
    }
}