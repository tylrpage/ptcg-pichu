using System;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Value;

namespace tylrpage.Pichu.PTCGEngine.Core.Effects
{
    public class DealDamageEffect : CardEffect
    {
        public NumberValue Amount;
        
        public override void Run(ResolverContext ctx, EffectSource source)
        {
            CardInstance attacker = ctx.WorkingState.FindCard(source.SourceCardId);
            CardDefinition attackerDefinition = ctx.CardDatabase.GetCardDefinition(attacker.CardDefinitionId);
            
            // Always attack the other player's active for now
            CardInstance target = ctx.WorkingState.GetPlayer(!source.IsPlayer1).Active;
            CardDefinition targetDefinition = ctx.CardDatabase.GetCardDefinition(target.CardDefinitionId);
            
            int baseDamage = Amount.Evaluate(ctx, source);
            
            // Weakness
            int afterWeakness = baseDamage;
            if (targetDefinition.Weakness != null && targetDefinition.Weakness == attackerDefinition.Type)
            {
                afterWeakness *= 2;
            }
            
            // Resistance
            int afterResistance = afterWeakness;
            if (targetDefinition.Resistance != null && targetDefinition.Resistance == attackerDefinition.Type)
            {
                afterResistance -= 30;
            }
            
            int finalDamage = Math.Max(0, afterResistance);
            
            int damageCounters = finalDamage / 10;
            target.DamageCounters += damageCounters;
            ctx.Events.Add(new DamageDealt()
            {
                AttackerId = source.SourceCardId,
                TargetId = target.Id,
                Base = baseDamage,
                AfterWeakness = afterWeakness,
                AfterResistance = afterResistance,
                Final = finalDamage,
                CountersPlaced = damageCounters,
            });
        }
    }
}