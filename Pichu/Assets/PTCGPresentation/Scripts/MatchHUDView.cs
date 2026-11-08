using System;
using TMPro;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;
using UnityEngine.UI;

public class MatchHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _turnAndPlayerText;
    [SerializeField] private TMP_Text _phaseAndSeedText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _player1Color;
    [SerializeField] private Color _player2Color;

    public void Render(GameState state, ulong seed)
    {
        bool isPlayer1Turn = state.PendingChoice?.IsPlayer1 ?? state.IsPlayer1Turn;
        _backgroundImage.color = isPlayer1Turn ? _player1Color : _player2Color;
        _turnAndPlayerText.text = $"Turn: {state.TurnNumber}, Player {(isPlayer1Turn ? 1 : 2)}";
        
        _phaseAndSeedText.text = $"Phase: {state.Phase}, Seed: {seed}";
    }
}
