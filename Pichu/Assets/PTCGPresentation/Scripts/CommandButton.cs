using TMPro;
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
                _text.text = $"Place basic {command.CardId}";
                break;
            case CommandType.ManualAttachEnergy:
                _text.text = $"Attach energy {command.CardId}->{command.TargetId}";
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
}
