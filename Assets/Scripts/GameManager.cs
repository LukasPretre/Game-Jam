using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool itemBouteille_Ramasse = false;
    public bool itemGrappin_Ramasse = false;

    public delegate void InventaireChangeDelegate();
    public static event InventaireChangeDelegate OnInventaireChanged;

    public void ActiverItem(string nomItem)
    {
        if (nomItem == "Bouteille")
        {
            itemBouteille_Ramasse = true;
            Debug.Log("Buff Bouteille activé !");
        }

        if (nomItem == "Grappin")
        {
            itemGrappin_Ramasse = true;
            Debug.Log("Grappin activé !");
        }

        // Notifie l'inventaire
        OnInventaireChanged?.Invoke();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}