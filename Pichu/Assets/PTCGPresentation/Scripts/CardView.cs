using TMPro;
using tylrpage.Pichu.PTCGEngine.CardDatabase;
using tylrpage.Pichu.PTCGEngine.Core;
using tylrpage.Pichu.PTCGEngine.Core.Rules;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _idText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Transform _energyContainer;
    [SerializeField] private SerializableDictionary<Element, GameObject> _elementTokenPrefabs;

    public void Bind(CardInstance cardInstance, GameState state, ICardDatabase db)
    {
        // Active could be null during setup
        gameObject.SetActive(cardInstance != null);
        if (cardInstance == null)
        {
            return;
        }
        
        _idText.text = cardInstance.Id.ToString();
        _nameText.text = db.GetCardDefinition(cardInstance.CardDefinitionId).Name;

        _hpText.gameObject.SetActive(false);
        if (CardQueries.IsPokemon(cardInstance, state, db))
        {
            _hpText.gameObject.SetActive(true);
            _hpText.text =
                $"{CardQueries.GetRemainingHP(cardInstance, state, db)}/{CardQueries.GetMaxHP(cardInstance, state, db)}";
        }

        // Clear energies
        foreach (Transform child in _energyContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (CardInstance energy in cardInstance.AttachedEnergy)
        {
            Element energyType = db.GetCardDefinition(energy.CardDefinitionId).Type;
            if (!_elementTokenPrefabs.TryGetValue(energyType, out GameObject tokenPrefab))
            {
                Debug.LogError($"Could not represent energy token, missing prefab, element: {energyType}");
                continue;
            }

            Instantiate(tokenPrefab, _energyContainer);
        }
    }
}
