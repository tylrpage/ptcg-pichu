using System;
using TMPro;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_InputField _seedInput;

    private MatchController _matchController;

    public void Render(MatchController matchController)
    {
        _matchController = matchController;

        if (matchController.WaitingToStart)
        {
            gameObject.SetActive(true);
            _text.text = $"Waiting to start game";
            return;
        }
        
        if (matchController.GameState.Phase != GamePhase.GameOver)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        if (!matchController.GameState.Player1Won.HasValue)
        {
            _text.text = $"Tie!";
        }
        else
        {
            _text.text = $"Player {(matchController.GameState.Player1Won.Value ? "1" : "2")} Won!";
        }
    }

    public void OnResetClicked()
    {
        ulong seed = 0;
        if (ulong.TryParse(_seedInput.text, out ulong inputSeed))
        {
            seed = inputSeed;
        }
        
        _matchController.ResetGame(seed);
    }
}
