using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Effects;
using tylrpage.Pichu.PTCGEngine.Core.Value;

namespace tylrpage.Pichu.PTCGEngine.CardDatabase
{
    // Creates a temporary card set for the engine and presentation layer to share 
    public class StarterCardSet
    {
        public const string PichuId = "dri_071";
        public const string MegaRayquazaExId = "dlr_084";
        public const string LightningId = "mee_004";
        public const string FireId = "mee_002";

        public static InMemoryCardDatabase CreateDatabase()
        { 
            InMemoryCardDatabase db = new InMemoryCardDatabase();
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = PichuId,
                Name = "Ethan's Pichu",
                Format = CardFormat.Pokemon,
                HP = 30,
                Stage = PokemonStage.Basic,
                Type = Element.Lightning,
                Weakness = Element.Fighting,
                Resistance = null,
                RetreatCost = 0,
                PrizeValue = 1,
                Attacks = new()
                {
                    new Attack()
                    {
                        Name = "Zapping Draw",
                        Cost = new(),
                        Effects = new List<CardEffect>()
                        {
                            new DealDamageEffect()
                            {
                                Amount = new ConstantValue()
                                {
                                    Value = 30
                                }
                            },
                            new DrawCardsEffect()
                            {
                                Count = new ConstantValue()
                                {
                                    Value = 1,
                                }
                            }
                        }
                    }
                }
            });
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = MegaRayquazaExId,
                Name = "Mega Rayquaza ex",
                Format = CardFormat.Pokemon,
                HP = 280,
                Stage = PokemonStage.Basic,
                Type = Element.Colorless,
                Weakness = Element.Lightning,
                Resistance = Element.Fighting,
                RetreatCost = 2,
                PrizeValue = 3,
                Attacks = new List<Attack>()
                {
                    new Attack()
                    {
                        Name = "Storm Emerald",
                        Cost = new()
                        {
                            Element.Fire,
                            Element.Lightning,
                            Element.Colorless,
                        },
                        Effects = new List<CardEffect>()
                        {
                            new DealDamageEffect()
                            {
                                Amount = new PerEnergyInPlay()
                                {
                                    PerEnergy = 50
                                }
                            }
                        }
                    }
                },
                Ability = new Ability()
                {
                    Name = "Ruler's Roar",
                    Trigger = TriggerType.PlayedToBenchFromHand,
                    Scope = TriggerScope.Self,
                    Effects = new List<CardEffect>()
                    {
                        new AttachEnergyFromTopDeckEffect()
                        {
                            LookCount = 4,
                        }
                    }
                }
            });
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = LightningId,
                Name = "Lightning Energy",
                Format = CardFormat.Energy,
                Type = Element.Lightning,
            });
            db.AddDefinition(new CardDefinition()
            {
                CardDefinitionId = FireId,
                Name = "Fire Energy",
                Format = CardFormat.Energy,
                Type = Element.Fire,
            });

            return db;
        }

        public static List<string> Deck(int pichus, int rayquazas, int lightningEnergies, int fireEnergies)
        {
            List<string> deck = new();
            for (int i = 0; i < pichus; i++)
            {
                deck.Add(PichuId);
            }
            for (int i = 0; i < rayquazas; i++)
            {
                deck.Add(MegaRayquazaExId);
            }
            for (int i = 0; i < lightningEnergies; i++)
            {
                deck.Add(LightningId);
            }
            for (int i = 0; i < fireEnergies; i++)
            {
                deck.Add(FireId);
            }

            return deck;
        }
    }
}