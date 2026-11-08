using System;
using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Effects;

namespace tylrpage.Pichu.PTCGEngine.Core.Resolution
{
    public class TriggerStep : GameStep
    {
        public TriggerType Type;
        public int CardId;
        public bool IsPlayer1;
        
        public override void Run(ResolverContext ctx)
        {
            List<CardInstance> pokemonInPlay = new();
            for (int i = 0; i < 2; i++)
            {
                PlayerState player = ctx.WorkingState.GetPlayer(i == 0);
                if (player.Active != null)
                {
                    pokemonInPlay.Add(player.Active);   
                }
                pokemonInPlay.AddRange(player.Bench);
            }

            // Check all pokemon in play for an ability with this trigger
            foreach (CardInstance pokemon in pokemonInPlay)
            {
                CardDefinition definition = ctx.CardDatabase.GetCardDefinition(pokemon.CardDefinitionId);
                if (definition.Ability != null && definition.Ability.Trigger == Type &&
                    IsInScope(pokemon, CardId, definition.Ability.Scope))
                {
                    // Queue each effect
                    List<GameStep> steps = new();
                    foreach (CardEffect effect in definition.Ability.Effects)
                    {
                        steps.Add(new RunEffectStep()
                        {
                            Effect = effect,
                            Source = new EffectSource()
                            {
                                IsPlayer1 = pokemon.IsOwnedByPlayer1,
                                SourceCardId = pokemon.Id,
                            }
                        });
                    }
                    ctx.QueueNext(steps);
                    
                    ctx.Events.Add(new AbilityTriggered()
                    {
                        CardId = pokemon.Id,
                    });
                }
            }
        }

        private bool IsInScope(CardInstance card, int cardId, TriggerScope scope)
        {
            if (scope == TriggerScope.Any)
            {
                return true;
            }
            else if (scope == TriggerScope.Self)
            {
                return cardId == card.Id;
            }

            throw new InvalidOperationException($"Unhandled scope: {scope}");
        }
    }
}