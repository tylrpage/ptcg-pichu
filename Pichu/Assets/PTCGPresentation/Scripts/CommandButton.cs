using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class CommandButton : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private PlayerCommand _command;
    private MatchController _controller;

    public void Bind(PlayerCommand command, MatchController controller)
    {
        _command = command;
        _controller = controller;
        
        switch (command.Type)
        {
            case CommandType.None:
                _text.text = $"[None]";
                break;
            case CommandType.EndTurn:
                _text.text = $"End Turn";
                break;
            case CommandType.PlaceBasic:
                _text.text = $"Place {GetCardDisplayName(command.CardId, controller.GameState, controller.Db)}";
                break;
            case CommandType.ManualAttachEnergy:
                _text.text = $"Attach {GetCardDisplayName(command.CardId, controller.GameState, controller.Db)} to {GetCardDisplayName(command.TargetId, controller.GameState, controller.Db)}";
                break;
        }
    }

    public void OnClick()
    {
        if (_command.Type == CommandType.None || _controller == null)
        {
            return;
        }
        
        _controller.Apply(_command);
    }
    
    private string GetCardName(int cardId, GameState state, ICardDatabase db)
    {
        return db.GetCardDefinition(state.FindCard(cardId).CardDefinitionId).Name;
    }

    private string GetCardDisplayName(int cardId, GameState state, ICardDatabase db)
    {
        return $"{GetCardName(cardId, state, db)} ({cardId})";
    }
}
