using System;
using UnityEngine;

// Top level canvas renderer
public class MatchView : MonoBehaviour
{
    [SerializeField] private MatchController _matchController;
    [SerializeField] private PlayerBoardView _player1BoardView;
    [SerializeField] private PlayerBoardView _player2BoardView;

    private void OnEnable()
    {
        _matchController.StateChanged += OnStateChanged;
        if (_matchController.IsSetup)
        {
            OnStateChanged();   
        }
    }

    private void OnDisable()
    {
        _matchController.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged()
    {
        _player1BoardView.Render(true, _matchController.GameState, _matchController.Db);
        _player2BoardView.Render(false, _matchController.GameState, _matchController.Db);
    }
}
