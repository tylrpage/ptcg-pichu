using System;
using TMPro;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class MatchHUDView : MonoBehaviour
{
    [SerializeField] private TMP_Text _turnAndPlayerText;
    [SerializeField] private TMP_Text _phaseAndSeedText;

    public void Render(GameState state, ulong seed)
    {
        _turnAndPlayerText.text = $"Turn: {state.TurnNumber}, Player {(state.IsPlayer1Turn ? 1 : 2)}";
        
        _phaseAndSeedText.text = $"Phase: {state.Phase}, Seed: {seed}";
    }
}
