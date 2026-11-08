using System;
using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Rules;

namespace tylrpage.Pichu.PTCGEngine.Core.Effects
{
    public class AttachEnergyFromTopDeckEffect : CardEffect
    {
        public int LookCount;
        
        public override void Run(ResolverContext ctx, EffectSource source)
        {
            PlayerState player = ctx.WorkingState.GetPlayer(source.IsPlayer1);
            
            List<CardInstance> cards = new();
            for (int i = 0; i < Math.Min(player.Deck.Count, LookCount); i++)
            {
                cards.Add(player.Deck[^(i + 1)]);
            }

            List<CardInstance> energies = cards.Where(x => CardQueries.IsBasicEnergy(x, ctx.WorkingState, ctx.CardDatabase))
                .ToList();
            
            ctx.WorkingState.PendingChoice = new ChoiceRequest()
            {
                Id = ctx.WorkingState.GetNextChoiceRequest(),
                Min = energies.Count > 0 ? 1 : 0,
                Max = 1,
                IsPlayer1 = source.IsPlayer1,
                Prompt = "Select energy to attach",
                CandidateIds = energies.Select(x => x.Id).ToList(),
            };
            ctx.QueueNext(new ApplyChoiceAttachEnergyFromTopDeckStep()
            {
                CardId = source.SourceCardId,
                ChoiceId = ctx.WorkingState.PendingChoice.Id,
                LookedAtCards = cards.Select(x => x.Id).ToList(),
            });
            ctx.Events.Add(new LookedAtCards()
            {
                CardIds = cards.Select(x => x.Id).ToList(),
            });
        }
    }
}