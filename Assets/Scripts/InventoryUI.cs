using UnityEngine;

public class InventaireUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemInventaire
    {
        public string nomItem;
        public Sprite sprite;
    }

    [SerializeField] private ItemInventaire[] listeItems;
    [SerializeField] private float espaceEntreItems = 1.5f;

    private void Start()
    {
        GameManager.OnInventaireChanged += AfficherInventaire;
        AfficherInventaire();
    }

    private void AfficherInventaire()
    {
        // Détruit les anciens
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Crée les nouveaux
        float posX = 7f;
        foreach (ItemInventaire item in listeItems)
        {
            if (EstPossede(item.nomItem))
            {
                GameObject newItem = new GameObject(item.nomItem);
                newItem.transform.parent = transform;
                newItem.transform.position = new Vector3(posX, 4.5f, 0);
                newItem.transform.localScale = Vector3.one * 0.5f;

                SpriteRenderer sr = newItem.AddComponent<SpriteRenderer>();
                sr.sprite = item.sprite;
                sr.sortingOrder = 100;

                posX += espaceEntreItems;
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