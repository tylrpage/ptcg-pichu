using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class PlayerBoardView : MonoBehaviour
{
    [SerializeField] private CardView _cardViewPrefab;
    [SerializeField] private CardView _active;
    [SerializeField] private Transform _benchContainer;
    [SerializeField] private Transform _handContainer;
    [SerializeField] private TMP_Text _deckText;
    [SerializeField] private TMP_Text _discardText;
    [SerializeField] private TMP_Text _prizeText;

    public void Render(bool isPlayer1, GameState state, ICardDatabase db)
    {
        PlayerState player = state.GetPlayer(isPlayer1);
        
        _active.Bind(player.Active, state, db);
        
        // Bench
        foreach (Transform card in _benchContainer)
        {
            Destroy(card.gameObject);
        }
        foreach (CardInstance card in player.Bench)
        {
            Instantiate(_cardViewPrefab, _benchContainer).Bind(card, state, db);
        }
        
        // Hand
        foreach (Transform card in _handContainer)
        {
            Destroy(card.gameObject);
        }
        foreach (CardInstance card in player.Hand)
        {
            Instantiate(_cardViewPrefab, _handContainer).Bind(card, state, db);
        }
        
        // Info
        _deckText.text = $"Deck: {player.Deck.Count}";
        _discardText.text = $"Discard: {player.Discard.Count}";
        _prizeText.text = $"Prize: {player.Prize.Count}";
    }
}
