using System;
using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChoiceView : MonoBehaviour
{
    [SerializeField] private TMP_Text _id;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Toggle _toggle;

    public void Bind(CardInstance card, GameState state, ICardDatabase db, Action<int, bool> onToggleChanged)
    {
        _id.text = card.Id.ToString();
        _name.text = db.GetCardDefinition(card.CardDefinitionId).Name;
        _toggle.onValueChanged.AddListener(x => onToggleChanged(card.Id, x));
    }

    private void OnDestroy()
    {
        _toggle.onValueChanged.RemoveAllListeners();
    }
}
