using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core.Effects;
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

            // Always attack the other player's active for now
            CardInstance target = ctx.WorkingState.GetPlayer(!IsPlayer1).Active;
            
            List<Attack> attacks = CardQueries.GetAttacks(attacker, ctx.WorkingState, ctx.CardDatabase);
            Attack attack = attacks[AttackIndexId];
            
            ctx.Events.Add(new AttackUsed()
            {
                IsPlayer1 = ctx.WorkingState.IsPlayer1Turn,
                AttackerId = AttackerId,
                TargetId = target.Id,
                AttackIndex = AttackIndexId,
            });

            List<GameStep> steps = new();
            foreach (CardEffect effect in attack.Effects)
            {
                steps.Add(new RunEffectStep()
                {
                    Effect = effect,
                    Source = new EffectSource()
                    {
                        IsPlayer1 = IsPlayer1,
                        SourceCardId = AttackerId,
                    }
                });
            }
            // Attack ends turns
            steps.Add(new EndTurnStep() { });
            
            ctx.QueueNext(steps);
        }
    }
}