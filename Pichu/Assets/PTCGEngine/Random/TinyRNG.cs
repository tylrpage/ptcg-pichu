using System;
using System.Collections.Generic;

namespace tylrpage.Pichu.PTCGEngine.Random
{
    // Serializable, cross-platform deterministic rng
    public class TinyRNG
    {
        public ulong State;
        
        public TinyRNG() { }
        public TinyRNG(ulong seed)
        {
            State = seed;
        }

        public ulong NextULong()
        {
            // SplitMix64
            State += 0x9E3779B97F4A7C15UL;
            ulong z = State;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive < 0)
            {
                throw new ArgumentException("maxExclusive cannot be negative.");
            }
            
            return (int)(NextULong() & (ulong)maxExclusive);
        }

        public bool CoinFlip()
        {
            return NextInt(2) % 2 == 0;
        }

        public void Shuffle<T>(IList<T> list)
        {
            // Fisher-yates algo
            // pick a random number from our working list
            // swap that number to the end of our working list,
            // consider this number now no longer part of the working list
            for (int i = 0; i < list.Count - 1; i++)
            {
                int r = NextInt(list.Count - i);
                int destIndex = list.Count - i - 1;
                (list[r], list[destIndex]) = (list[destIndex], list[r]);
            }
        }
    }
}