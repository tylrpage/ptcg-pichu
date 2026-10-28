using System;
using System.Collections.Generic;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

// Manages the collection of game event views
public class GameLogView : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private GameEventView _gameEventViewPrefab;

    public void Render(List<GameEvent> events)
    {
        foreach (Transform child in _container)
        {
            Destroy(child.gameObject);
        }
        
        // reverse order
        for (int i = events.Count - 1; i >= 0; i--)
        {
            GameEvent gameEvent = events[i];
            Instantiate(_gameEventViewPrefab, _container).Bind(gameEvent);
        }
    }
}
