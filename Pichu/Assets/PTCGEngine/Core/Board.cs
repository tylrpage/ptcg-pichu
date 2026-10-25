using System;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    // Board utility functions
    public static class Board
    {
        // The one place cards can get a new zone assigned, ensures their Zone prop is kept in sync
        internal static void AddCardInstanceToZone(GameState gameState, CardInstance card, PlayerState playerState, Zone zone)
        {
            switch (zone)
            {
                case Zone.Active:
                    card.Zone = Zone.Active;
                    playerState.Active = card;
                    break;
                case Zone.Deck:
                    card.Zone = Zone.Deck;
                    playerState.Deck.Add(card);
                    break;
                case Zone.Bench:
                    card.Zone = Zone.Bench;
                    playerState.Bench.Add(card);
                    break;
                case Zone.Prize:
                    card.Zone = Zone.Prize;
                    playerState.Prize.Add(card);
                    break;
                case Zone.Discard:
                    card.Zone = Zone.Discard;
                    playerState.Discard.Add(card);
                    break;
                case Zone.Hand:
                    card.Zone = Zone.Hand;
                    playerState.Hand.Add(card);
                    break;
                case Zone.Stadium:
                    card.Zone = Zone.Stadium;
                    gameState.Stadium = card;
                    break;
                default:
                    throw new ArgumentException("Unknown zone");
            }
        }
    }
}