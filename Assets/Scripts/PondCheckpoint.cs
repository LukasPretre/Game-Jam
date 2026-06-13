using UnityEngine;

public class PondCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie si c'est bien le joueur qui entre dans l'eau
        if (collision.CompareTag("Player"))
        {
            OxygenManager playerOxygen = collision.GetComponent<OxygenManager>();

            if (playerOxygen != null)
            {
                // LA MODIF EST ICI : 
                // Au lieu de transform.position (qui donne le centre de la Tilemap),
                // on utilise collision.transform.position (qui donne la position exacte du kamtar)

                Vector3 spawnPoint = new Vector3(
                    collision.transform.position.x,
                    collision.transform.position.y + 0.5f, // On le remonte un peu pour qu'il ne spawn pas sous le sol
                    collision.transform.position.z
                );

                playerOxygen.UpdateCheckpoint(spawnPoint);
            }
        }
    }
}