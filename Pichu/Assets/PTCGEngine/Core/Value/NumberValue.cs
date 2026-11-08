using tylrpage.Pichu.PTCGEngine.Core.Resolution;

namespace tylrpage.Pichu.PTCGEngine.Core.Value
{
    public abstract class NumberValue
    {
        public abstract int Evaluate(ResolverContext ctx, EffectSource source);
    }
}