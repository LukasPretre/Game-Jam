using UnityEngine;

public class ShowBouteille : MonoBehaviour
{
    private void Start()
    {
        GameManager.OnInventaireChanged += Afficher;
        Afficher();
    }

    private void Afficher()
    {
        gameObject.SetActive(GameManager.instance.itemBouteille_Ramasse);
    }
}