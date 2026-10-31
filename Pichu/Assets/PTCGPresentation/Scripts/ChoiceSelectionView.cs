using System;
using System.Collections.Generic;
using System.Linq;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceSelectionView : MonoBehaviour
{
    [SerializeField] private ChoiceView _choicePrefab;
    [SerializeField] private Transform _choicesContainer;
    [SerializeField] private Button _submitButton;

    private HashSet<int> _selectedCardIds = new();
    private PlayerCommand _chooseCommand;
    private bool _isPlayer1;
    private int _choiceRequestId;
    private MatchController _matchController;
    private GameState _state;
    private ICardDatabase _db;

    public void Render(MatchController matchController)
    {
        if (matchController.GameState.PendingChoice == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _matchController = matchController;
        _state = _matchController.GameState;
        _db = _matchController.Db;
        _isPlayer1 = matchController.GameState.PendingChoice.IsPlayer1;
        _choiceRequestId = matchController.GameState.PendingChoice.Id;
        
        gameObject.SetActive(true);

        _selectedCardIds.Clear();
        foreach (Transform child in _choicesContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (int candidateId in matchController.GameState.PendingChoice.CandidateIds)
        {
            CardInstance card = matchController.GameState.FindCard(candidateId);
            ChoiceView choiceView = Instantiate(_choicePrefab, _choicesContainer);
            choiceView.Bind(card, matchController.GameState, matchController.Db, OnToggleChanged);
        }

        UpdateValidity();
    }

    private void OnToggleChanged(int cardId, bool isOn)
    {
        if (isOn)
        {
            _selectedCardIds.Add(cardId);
        }
        else
        {
            _selectedCardIds.Remove(cardId);
        }

        UpdateValidity();
    }

    private void UpdateValidity()
    {
        if (_state == null || _db == null)
        {
            _submitButton.interactable = false;
            return;
        }
        
        // Update validity
        _chooseCommand = PlayerCommand.Choose(_isPlayer1, _choiceRequestId, _selectedCardIds.ToArray());
        RuleResult result = RuleChecker.Check(_chooseCommand, _state, _db);

        _submitButton.interactable = result == RuleResult.Ok;
    }

    public void Submit()
    {
        _matchController?.Apply(_chooseCommand);
    }
}
