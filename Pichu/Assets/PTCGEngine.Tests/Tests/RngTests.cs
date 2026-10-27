using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.Random;

namespace tylrpage.Pichu.PTCGEngine.Tests.Tests
{
    public class RngTests
    {
        [Test]
        public void Rng_DeterministicShuffle()
        {
            List<int> numbers = Enumerable.Range(0, 50).ToList();
            List<int> numbers2 = numbers.ToList();

            TinyRNG rng = new TinyRNG();
            rng.Shuffle(numbers);

            TinyRNG rng2 = new TinyRNG();
            rng2.Shuffle(numbers2);
            
            Assert.That(numbers.Count, Is.EqualTo(numbers2.Count));
            for (int i = 0; i < numbers.Count; i++)
            {
                Assert.That(numbers[i], Is.EqualTo(numbers2[i]));
            }
        }
    }
}