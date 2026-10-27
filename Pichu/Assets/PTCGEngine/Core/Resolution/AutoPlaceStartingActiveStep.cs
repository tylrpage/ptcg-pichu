using System;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    // temp step to get game setup with player input
    // puts the first basic in hand into active
    public class AutoPlaceStartingActiveStep : GameStep
    {
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            CardInstance firstBasic =
                player.Hand.FirstOrDefault(x => CardQueries.IsBasicPokemon(x, ctx.WorkingState, ctx.CardDatabase));
            if (firstBasic == null)
            {
                // todo: add mulligans
                throw new InvalidOperationException($"Player {(IsPlayer1 ? "1" : "2")} has no basics in hand");
            }
            ctx.QueueNext(new MoveCardStep()
            {
                CardId = firstBasic.Id,
                To = Zone.Active,
            });
        }
    }
}