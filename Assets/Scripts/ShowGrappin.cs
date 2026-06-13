using UnityEngine;

public class ShowGrappin : MonoBehaviour
{
    private void Start()
    {
        GameManager.OnInventaireChanged += Afficher;
        Afficher();
    }

    private void Afficher()
    {
        gameObject.SetActive(GameManager.instance.itemGrappin_Ramasse);
    }
}
