using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class SetPrizesStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            List<CardInstance> deck = player.Deck;
            
            for (int i = 0; i < CardQueries.StartingPrizeCardCount; i++)
            {
                // Check for deckout
                if (deck.Count == 0)
                {
                    throw new InvalidOperationException("Not enough cards in deck to set prizes");
                }

                CardInstance topCard = deck[^1];
                ctx.WorkingState.MoveCard(topCard, Zone.Prize);
                // todo: make an event for it
            }
        }
    }
}