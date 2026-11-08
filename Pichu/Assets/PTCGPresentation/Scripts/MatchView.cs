using UnityEngine;

// Top level canvas renderer
public class MatchView : MonoBehaviour
{
    [SerializeField] private MatchController _matchController;
    [SerializeField] private MatchHUDView _hudView;
    [SerializeField] private PlayerBoardView _player1BoardView;
    [SerializeField] private PlayerBoardView _player2BoardView;
    [SerializeField] private CommandListView _commandList;
    [SerializeField] private GameLogView _gameLog;
    [SerializeField] private OverlayController _overlay;
    [SerializeField] private ChoiceSelectionView _choiceSelection;

    private void OnEnable()
    {
        _matchController.StateChanged += OnStateChanged;
        if (_matchController.IsSetup)
        {
            OnStateChanged();   
        }
        else
        {
            _overlay.Render(_matchController);
        }
    }

    private void OnDisable()
    {
        _matchController.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged()
    {
        _hudView.Render(_matchController.GameState, _matchController.Seed);
        
        _player1BoardView.Render(true, _matchController.GameState, _matchController.Db);
        _player2BoardView.Render(false, _matchController.GameState, _matchController.Db);
        
        _commandList.Render(_matchController.GetLegalCommands(_matchController.GameState.IsPlayer1Turn), _matchController);
        _choiceSelection.Render(_matchController);
        _gameLog.Render(_matchController.Log, _matchController.GameState, _matchController.Db);
        
        _overlay.Render(_matchController);
    }
}
