using UnityEngine;
using UnityEngine.UI;

public class InventaireUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemInventaire
    {
        public string nomItem;
        public Sprite sprite;
    }

    [SerializeField] private Transform containerItems;
    [SerializeField] private GameObject prefabItemUI;
    [SerializeField] private ItemInventaire[] listeItems;

    private void Start()
    {
        RefreshInventaire();
        GameManager.OnInventaireChanged += RefreshInventaire;
    }

    private void RefreshInventaire()
    {
        // Vide le conteneur en partant du dernier enfant
        while (containerItems.childCount > 0)
        {
            DestroyImmediate(containerItems.GetChild(0).gameObject);
        }

        // Ajoute les items possédés
        foreach (ItemInventaire item in listeItems)
        {
            if (EstPossede(item.nomItem))
            {
                GameObject newItem = Instantiate(prefabItemUI, containerItems);
                Image img = newItem.GetComponent<Image>();
                if (img != null) img.sprite = item.sprite;

                Text txt = newItem.GetComponentInChildren<Text>();
                if (txt != null) txt.text = item.nomItem;
            }
        }
    }

    private bool EstPossede(string nomItem)
    {
        if (nomItem == "Bouteille") return GameManager.instance.itemBouteille_Ramasse;
        if (nomItem == "Grappin") return GameManager.instance.itemGrappin_Ramasse;
        return false;
    }
}