using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class ChoiceSelectionView : MonoBehaviour
{
    [SerializeField] private ChoiceView _choicePrefab;
    [SerializeField] private Transform _choicesContainer;

    public void Render(GameState state, ICardDatabase db)
    {
        if (state.PendingChoice == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);

        foreach (Transform child in _choicesContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (int candidateId in state.PendingChoice.CandidateIds)
        {
            CardInstance card = state.FindCard(candidateId);
            Instantiate(_choicePrefab, _choicesContainer).Bind(card, state, db);
        }
    }
}
