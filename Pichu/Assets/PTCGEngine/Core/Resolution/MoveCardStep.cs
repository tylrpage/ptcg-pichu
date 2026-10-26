namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class MoveCardStep : GameStep
    {
        public int CardId;
        public Zone To;
        
        public override void Run(ResolveContext ctx)
        {
            CardInstance card = ctx.WorkingState.FindCard(CardId);
            Zone from = card.Zone;
            int parentId = card.ParentCardInstanceId;
            
            ctx.WorkingState.MoveCard(card, To);
            ctx.Events.Add(new CardMoved()
            {
                CardId = CardId,
                To = To,
                From = from,
                FromParentId = parentId,
            });
        }
    }
}