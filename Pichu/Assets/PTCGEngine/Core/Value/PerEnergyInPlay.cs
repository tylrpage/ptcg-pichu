using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Value
{
    public class PerEnergyInPlay : NumberValue
    {
        public int PerEnergy;
        
        public override int Evaluate(ResolverContext ctx, EffectSource source)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(source.IsPlayer1);
            List<CardInstance> pokemonInPlay = player.Bench
                .Append(player.Active)
                .ToList();

            int energyCount = 0;
            foreach (CardInstance pokemon in pokemonInPlay)
            {
                energyCount += CardQueries.GetAttachedEnergyCounts(pokemon, ctx.WorkingState, ctx.CardDatabase).Values.Sum();
            }

            return energyCount * PerEnergy;
        }
    }
}