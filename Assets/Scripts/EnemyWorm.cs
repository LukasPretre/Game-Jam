using UnityEngine;

public class EnemyWorm : MonoBehaviour
{
    [Header("Patrouille")]
    public float speed = 2f;
    public float patrolDistance = 3f; // Jusqu'où il va avant de faire demi-tour

    private float startX;
    private bool movingRight = true;

    [Header("Dégâts")]
    public float oxygenDamage = 2f; // Combien d'air il enlève au joueur

    void Start()
    {
        // On mémorise la position de départ du ver quand le jeu se lance
        startX = transform.position.x;
    }

    void Update()
    {
        if (movingRight)
        {
            // Il avance vers la droite
            transform.Translate(Vector2.right * speed * Time.deltaTime);

            // S'il a dépassé sa distance max, il fait demi-tour
            if (transform.position.x >= startX + patrolDistance)
            {
                Flip();
            }
        }
        else
        {
            // Il avance vers la gauche
            transform.Translate(Vector2.left * speed * Time.deltaTime);

            // S'il revient trop loin en arrière, il fait demi-tour
            if (transform.position.x <= startX - patrolDistance)
            {
                Flip();
            }
        }
    }

    // Fonction pour retourner le ver visuellement
    void Flip()
    {
        movingRight = !movingRight;

        // On inverse l'échelle (Scale) sur l'axe X pour que le sprite regarde dans l'autre sens
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    // Quand le ver rentre en collision avec quelque chose
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On vérifie si l'objet touché a le Tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // On récupère le script d'oxygène du joueur
            OxygenManager playerOxygen = collision.gameObject.GetComponent<OxygenManager>();

            // S'il en a bien un, on lui inflige les dégâts
            if (playerOxygen != null)
            {
                playerOxygen.LoseOxygen(oxygenDamage);
            }
        }
    }
}