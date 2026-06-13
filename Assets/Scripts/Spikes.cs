using UnityEngine;

public class Spikes : MonoBehaviour
{
    // Cette fonction se déclenche quand un objet physique touche les pics
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On vérifie si c'est bien le kamtar qui s'empale
        if (collision.gameObject.CompareTag("Player"))
        {
            // On récupère ton script qui gère la vie
            OxygenManager playerOxygen = collision.gameObject.GetComponent<OxygenManager>();

            if (playerOxygen != null)
            {
                // On déclenche directement ta fonction de mort (et donc le respawn à l'étang !)
                playerOxygen.Die();
            }
        }
    }
}