using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class KnockOutStep : GameStep
    {
        public int CardId;
        
        public override void Run(ResolverContext ctx)
        {
            CardInstance card = ctx.WorkingState.FindCard(CardId);
            PlayerState otherPlayer = ctx.WorkingState.GetPlayer(!card.IsOwnedByPlayer1);
            otherPlayer.OwedPrizeCards += CardQueries.GetPrizeValue(card, ctx.WorkingState, ctx.CardDatabase);
            
            ctx.QueueNext(new MoveCardStep()
            {
                CardId = CardId,
                To = Zone.Discard,
            });
            
            ctx.Events.Add(new KnockedOut()
            {
                CardId = CardId,
            });
        }
    }
}