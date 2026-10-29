using System;
using System.Collections.Generic;
using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;

// Represents a single game event
public class GameEventView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Bind(GameEvent gameEvent, GameState state, ICardDatabase db)
    {
        string text = "[None]";
        switch (gameEvent)
        {
            case CardMoved cardMoved:
                text = $"{GetCardDisplayName(cardMoved.CardId, state, db)} moved from {cardMoved.From} to {cardMoved.To}";
                break;
            case CardDrawn cardDrawn:
                text = $"{GetPlayerString(cardDrawn.IsPlayer1)} drew {GetCardDisplayName(cardDrawn.CardId, state, db)}";
                break;
            case DeckShuffled deckShuffled:
                text = $"{GetPlayerString(deckShuffled.IsPlayer1)} shuffled deck";
                break;
            case EnergyAttached energyAttached:
                text = $"{GetCardDisplayName(energyAttached.EnergyId, state, db)} attached to {GetCardDisplayName(energyAttached.TargetId, state, db)}";
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

    private string GetCardName(int cardId, GameState state, ICardDatabase db)
    {
        return db.GetCardDefinition(state.FindCard(cardId).CardDefinitionId).Name;
    }

    private string GetCardDisplayName(int cardId, GameState state, ICardDatabase db)
    {
        return $"{GetCardName(cardId, state, db)} ({cardId})";
    }

    private string GetPlayerString(bool isPlayer1)
    {
        return isPlayer1 ? "Player 1" : "Player 2";
    }
}
