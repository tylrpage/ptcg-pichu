using TMPro;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Render(GameState state)
    {
        if (state.Phase != GamePhase.GameOver)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        if (!state.Player1Won.HasValue)
        {
            _text.text = $"Tie!";
        }
        else
        {
            _text.text = $"Player {(state.Player1Won.Value ? "1" : "2")} Won!";
        }
    }
}
