using System;
using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Core
{
    public class PlayerState
    {
        // Top of the deck is the last card
        public List<CardInstance> Deck = new();
        public List<CardInstance> Hand = new();
        public List<CardInstance> Discard = new();
        public List<CardInstance> Prize = new();
        public List<CardInstance> Bench = new();
        public CardInstance Active;
        public bool FailedToDrawCard;
    }
}