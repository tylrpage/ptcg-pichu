using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class AttackStep : GameStep
    {
        public bool IsPlayer1;
        public int AttackerId;
        public int AttackIndexId;
        
        public override void Run(ResolverContext ctx)
        {
            CardInstance attacker = ctx.WorkingState.FindCard(AttackerId);
            CardDefinition attackerDefinition = ctx.CardDatabase.GetCardDefinition(attacker.CardDefinitionId);

            // Always attack the other player's active for now
            CardInstance target = ctx.WorkingState.GetPlayer(!IsPlayer1).Active;
            CardDefinition targetDefinition = ctx.CardDatabase.GetCardDefinition(target.CardDefinitionId);
            
            List<Attack> attacks = CardQueries.GetAttacks(attacker, ctx.WorkingState, ctx.CardDatabase);
            Attack attack = attacks[AttackIndexId];
            
            ctx.Events.Add(new AttackUsed()
            {
                IsPlayer1 = ctx.WorkingState.IsPlayer1Turn,
                AttackerId = AttackerId,
                TargetId = target.Id,
                AttackIndex = AttackIndexId,
            });

            int baseDamage = attack.Damage;
            
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
                AttackerId = AttackerId,
                TargetId = target.Id,
                Base = baseDamage,
                AfterWeakness = afterWeakness,
                AfterResistance = afterResistance,
                Final = finalDamage,
                CountersPlaced = damageCounters,
            });
            
            // Attack ends turns
            ctx.QueueNext(new EndTurnStep() { });
        }
    }
}