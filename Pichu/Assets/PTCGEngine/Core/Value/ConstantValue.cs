using tylrpage.Pichu.PTCGEngine.Core.Resolution;

namespace tylrpage.Pichu.PTCGEngine.Core.Value
{
    public class ConstantValue : NumberValue
    {
        public int Value;
        
        public override int Evaluate(ResolverContext ctx, EffectSource source)
        {
            return Value;
        }
    }
}