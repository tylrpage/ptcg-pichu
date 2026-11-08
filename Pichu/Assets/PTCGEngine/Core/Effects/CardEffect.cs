using tylrpage.Pichu.PTCGEngine.Core.Resolution;

namespace tylrpage.Pichu.PTCGEngine.Core.Effects
{
    public abstract class CardEffect
    {
        public abstract void Run(ResolverContext ctx, EffectSource source);
    }
}