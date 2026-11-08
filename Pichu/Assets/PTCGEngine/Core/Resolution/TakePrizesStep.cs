using System;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class TakePrizesStep : GameStep
    {
        public bool IsPlayer1;
        public int Prizes;
        
        public override void Run(ResolverContext ctx)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(IsPlayer1);
            int prizesCanTake = Math.Min(Prizes, player.Prize.Count);
            for (int i = 0; i < prizesCanTake; i++)
            {
                ctx.QueueNext(new MoveCardStep()
                {
                    CardId = player.Prize[i].Id,
                    To = Zone.Hand,
                });
            }
            
            ctx.Events.Add(new PrizesTaken()
            {
                IsPlayer1 = IsPlayer1,
                Prizes = prizesCanTake,
            });

            player.OwedPrizeCards = 0;
        }
    }
}