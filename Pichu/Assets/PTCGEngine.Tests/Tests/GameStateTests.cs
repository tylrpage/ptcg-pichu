using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;

namespace tylrpage.Pichu.PTCGEngine.Tests
{
    public class GameStateTests
    {
        private InMemoryCardDatabase _db;
        private GameState _state;
        
        [SetUp]
        public void Setup()
        {
            _db = TestCards.CreateDatabase();
            _state = GameStateFactory.Create(_db, TestCards.Deck(40, 20), TestCards.Deck(40, 20));
        }

        [Test]
        public void Create_All60CardsInDeck()
        {
            foreach (CardInstance card in _state.GetAllCards())
            {
                Assert.That(card.Zone, Is.EqualTo(Zone.Deck));
            }
            
            Assert.That(_state.Player1State.Deck.Count, Is.EqualTo(60));
            Assert.That(_state.Player2State.Deck.Count, Is.EqualTo(60));
            // Assert each card is owned by correct player
            foreach (CardInstance player1Card in _state.Player1State.Deck)
            {
                Assert.That(player1Card.IsOwnedByPlayer1, Is.True);
            }
            foreach (CardInstance player2Card in _state.Player2State.Deck)
            {
                Assert.That(player2Card.IsOwnedByPlayer1, Is.False);
            }
        }

        [Test]
        public void Create_IdsAreUnique()
        {
            IEnumerable<CardInstance> allCards = _state.GetAllCards().ToList();
            Assert.That(allCards.Select(x => x.Id).Distinct().Count(),
                Is.EqualTo(allCards.Count()));
        }

        [Test]
        public void Create_IsDeterministic()
        {
            GameState game2 = GameStateFactory.Create(_db, TestCards.Deck(40, 20), TestCards.Deck(40, 20));
            
            List<CardInstance> game1Cards = _state.GetAllCards().ToList();
            List<CardInstance> game2Cards = game2.GetAllCards().ToList();
            
            Assert.That(game1Cards.Count, Is.EqualTo(game2Cards.Count));
            for (int i = 0; i < game1Cards.Count; i++)
            {
                Assert.That(game1Cards[i].Id, Is.EqualTo(game2Cards[i].Id));
                Assert.That(game1Cards[i].CardDefinitionId, Is.EqualTo(game2Cards[i].CardDefinitionId));
            }
        }

        [Test]
        public void AllCards_IncludesAttachedCards()
        {
            CardInstance energy = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.GrassId);
            CardInstance pokemon = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.TreeckoId);
            
            // move pokemon to bench, so it can have attachment
            _state.Player1State.Deck.Remove(pokemon);
            _state.Player1State.Bench.Add(pokemon);
            pokemon.Zone = Zone.Bench;
            
            // manually attach energy
            // todo: replace with usage of attach function
            _state.Player1State.Deck.Remove(energy);
            energy.ParentCardInstanceId = pokemon.Id;
            pokemon.AttachedEnergy.Add(energy);
            
            Assert.That(energy, Is.Not.Null);
            Assert.That(_state.GetAllCards().Count(), Is.EqualTo(120));
        }
        
        [Test]
        public void FindCard_FindsAttachedCard()
        {
            CardInstance energy = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.GrassId);
            CardInstance pokemon = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.TreeckoId);
            
            // move pokemon to bench, so it can have attachment
            _state.Player1State.Deck.Remove(pokemon);
            _state.Player1State.Bench.Add(pokemon);
            pokemon.Zone = Zone.Bench;
            
            // manually attach energy
            // todo: replace with usage of attach function
            _state.Player1State.Deck.Remove(energy);
            energy.ParentCardInstanceId = pokemon.Id;
            pokemon.AttachedEnergy.Add(energy);
            
            // assert that the attached energy could still be found
            Assert.That(_state.FindCard(energy.Id), Is.Not.Null);
        }

        [Test]
        public void Clone_PreservesAttachments()
        {
            CardInstance energy = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.GrassId);
            CardInstance pokemon = _state.Player1State.Deck.First(x => x.CardDefinitionId == TestCards.TreeckoId);
            
            // move pokemon to bench, so it can have attachment
            _state.Player1State.Deck.Remove(pokemon);
            _state.Player1State.Bench.Add(pokemon);
            pokemon.Zone = Zone.Bench;
            
            // manually attach energy
            // todo: replace with usage of attach function
            _state.Player1State.Deck.Remove(energy);
            energy.ParentCardInstanceId = pokemon.Id;
            pokemon.AttachedEnergy.Add(energy);
            
            // clone
            GameState clone = _state.Clone();
            
            CardInstance clonedEnergy = clone.FindCard(energy.Id);
            CardInstance clonedPokemon = clone.FindCard(pokemon.Id);
            
            Assert.That(clonedEnergy, Is.Not.Null);
            Assert.That(clonedPokemon, Is.Not.Null);
            // Assert for same id but different objects
            Assert.That(clonedEnergy, Is.Not.SameAs(energy));
            Assert.That(clonedPokemon, Is.Not.SameAs(pokemon));
            // Assert that it is still attached
            Assert.That(clonedPokemon.AttachedEnergy[0].Id, Is.EqualTo(clonedEnergy.Id));
            Assert.That(clonedEnergy.ParentCardInstanceId, Is.EqualTo(clonedPokemon.Id));
        }

        [Test]
        public void Clone_IsIndependent()
        {
            GameState clone = _state.Clone();
            
            // Move a card from deck to hand in the clone
            int i = 0;
            CardInstance card = clone.Player1State.Deck[i];
            clone.Player1State.Deck.RemoveAt(i);
            clone.Player1State.Hand.Add(card);
            
            // original shouldn't have changed
            Assert.That(clone.Player1State.Deck.Count, Is.EqualTo(59));
            Assert.That(clone.Player1State.Hand.Count, Is.EqualTo(1));
            Assert.That(_state.Player1State.Deck.Count, Is.EqualTo(60));
            Assert.That(_state.Player1State.Hand.Count, Is.EqualTo(0));
        }
    }
}