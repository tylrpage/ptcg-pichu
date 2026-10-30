using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceView : MonoBehaviour
{
    [SerializeField] private TMP_Text _id;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Toggle _toggle;

    public void Bind(CardInstance card, GameState state, ICardDatabase db)
    {
        _id.text = card.Id.ToString();
        _name.text = db.GetCardDefinition(card.CardDefinitionId).Name;
    }
}
