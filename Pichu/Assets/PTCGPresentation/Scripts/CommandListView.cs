using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

public class CommandListView : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private CommandButton _commandButtonPrefab;

    public void Render(List<PlayerCommand> commands, MatchController controller)
    {
        foreach (Transform child in _container)
        {
            Destroy(child.gameObject);
        }
        foreach (PlayerCommand command in commands)
        {
            Instantiate(_commandButtonPrefab, _container).Bind(command, controller);
        }
    }
}
