using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Value;

namespace tylrpage.Pichu.PTCGEngine.Core.Effects
{
    public class DrawCardsEffect : CardEffect
    {
        public NumberValue Count;
        
        public override void Run(ResolverContext ctx, EffectSource source)
        {
            ctx.QueueNext(new DrawStep()
            {
                IsPlayer1 = source.IsPlayer1,
                Number = Count.Evaluate(ctx, source),
                IsTurnDraw = false,
            });
        }
    }
}