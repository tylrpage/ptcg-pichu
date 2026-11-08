using tylrpage.Pichu.PTCGEngine.Core.Effects;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class RunEffectStep : GameStep
    {
        public EffectSource Source;
        public CardEffect Effect;
        
        public override void Run(ResolverContext ctx)
        {
            Effect.Run(ctx, Source);
        }
    }
}