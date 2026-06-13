using UnityEngine;

public class TakeItem : MonoBehaviour
{
    [Header("Paramètres de ramassage")]
    [Tooltip("Laissez vide si le script est directement sur l'objet à détruire")]
    public GameObject itemToDestroy;

    [Header("Configuration")]
    [Tooltip("Le nom de la variable dans le GameManager qui doit passer à true")]
    public string nomDeLaVariable;

    private void Start()
    {
        // Si aucune référence n'est donnée, on considère que c'est cet objet lui-même
        if (itemToDestroy == null)
        {
            itemToDestroy = this.gameObject;
        }
    }

    // Cette méthode est appelée automatiquement par Unity lors d'une collision trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifie si l'objet qui entre est bien le joueur
        if (collision.CompareTag("Player"))
        {
            RamasserObjet();
        }
    }

    private void RamasserObjet()
    {
        // On demande au GameManager d'activer la variable correspondante via son nom
        if (GameManager.instance != null && !string.IsNullOrEmpty(nomDeLaVariable))
        {
            GameManager.instance.ActiverItem(nomDeLaVariable);
        }

        Debug.Log("Objet ramassé : " + nomDeLaVariable);
        Destroy(itemToDestroy);
    }
}