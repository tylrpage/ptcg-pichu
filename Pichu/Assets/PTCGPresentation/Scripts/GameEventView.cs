using System;
using System.Collections.Generic;
using TMPro;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

// Represents a single game event
public class GameEventView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Bind(GameEvent gameEvent)
    {
        string text = "[None]";
        switch (gameEvent)
        {
            case CardMoved cardMoved:
                text = $"Card {cardMoved.CardId} moved from {cardMoved.From} to {cardMoved.To}";
                break;
            case CardDrawn cardDrawn:
                text = $"{GetPlayerString(cardDrawn.IsPlayer1)} drew card {cardDrawn.CardId}";
                break;
            case DeckShuffled deckShuffled:
                text = $"{GetPlayerString(deckShuffled.IsPlayer1)} shuffled deck";
                break;
            case EnergyAttached energyAttached:
                text = $"Energy {energyAttached.EnergyId} attached to {energyAttached.TargetId}";
                break;
            case GameStarted gameStarted:
                text = $"The game has started!";
                break;
            case PlayerLost playerLost:
                text = $"{GetPlayerString(playerLost.IsPlayer1)} has lost!";
                break;
            case TurnEnded turnEnded:
                text = $"{GetPlayerString(turnEnded.IsPlayer1)}'s turn has ended";
                break;
            case TurnStarted turnStarted:
                text = $"{GetPlayerString(turnStarted.IsPlayer1)}'s turn has started";
                break;
        }

        _text.text = text;
    }

    private string GetPlayerString(bool isPlayer1)
    {
        return isPlayer1 ? "Player 1" : "Player 2";
    }
}
