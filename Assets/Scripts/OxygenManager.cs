using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OxygenManager : MonoBehaviour
{
    [Header("Parametres oxygene")]
    public float maxOxygen = 10f;
    private float currentOxygen;
    private bool isInWatter = false;

    [Header("Bonus d'amélioration")]
    public float oxygenUpgradeAmount = 10f;
    private bool isUpgraded = false;

    [Header("Vitesse de recharge de l'oxygene")]
    public float rechargeSpeed = 5f;

    [Header("Consommation basée sur la vitesse")]
    public float baseConsumption = 1f;
    public float velocityImpact = 0.05f;

    [Header("Consommation des sauts")] // NOUVEAU
    public float jumpConsumption = 1f; // Oxygène consommé par saut
    public float wallJumpConsumption = 1.5f; // Wall jump consomme plus
    public float doubleJumpConsumption = 1.2f; // Double jump

    [Header("Interface Graphique UI")]
    public Slider oxygenBar;

    public Vector3 respawnPosition;
    private Rigidbody2D rb;

    private Animator anim;
    public bool isDying = false;

    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip waterSound;

    void Start()
    {
        if (oxygenBar == null)
        {
            GameObject sliderTrouve = GameObject.Find("Slider");
            if (sliderTrouve != null)
            {
                oxygenBar = sliderTrouve.GetComponent<Slider>();
            }
            else
            {
                Debug.LogError("j'ai pas trouvé la barre d'oxygène !");
            }
        }
        currentOxygen = maxOxygen;
        rb = GetComponent<Rigidbody2D>();

        respawnPosition = transform.position;

        if (oxygenBar != null)
        {
            oxygenBar.minValue = 0f;
            oxygenBar.maxValue = maxOxygen;
            oxygenBar.value = maxOxygen;
        }
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Si l'item a été ramassé, on augmente l'oxygène
        if (GameManager.instance != null && GameManager.instance.itemBouteille_Ramasse && !isUpgraded)
        {
            isUpgraded = true;
            maxOxygen += oxygenUpgradeAmount;
            currentOxygen += oxygenUpgradeAmount;

            if (oxygenBar != null)
            {
                oxygenBar.maxValue = maxOxygen;
                oxygenBar.value = currentOxygen;
            }
            Debug.Log("Oxygène max augmenté !");
        }
    }

    // Cette fonction est appelée par le PlayerMovement à chaque frame
    public void ManageOxygen(float playerSpeed)
    {
        // si on est pas dans l'eau -> ca baisse en fonction de la vitesse
        if (!isInWatter)
        {
            float consumption = baseConsumption + (playerSpeed * velocityImpact);
            currentOxygen -= consumption * Time.deltaTime;

            if (currentOxygen <= 0)
            {
                currentOxygen = 0;
                Die();
            }
        }
        // si on dans l'eau -> ca remonte
        else
        {
            currentOxygen += rechargeSpeed * Time.deltaTime;

            if (currentOxygen > maxOxygen)
            {
                currentOxygen = maxOxygen;
            }
        }

        if (oxygenBar != null)
        {
            oxygenBar.value = currentOxygen;
        }
    }

    // NOUVEAU - Consomme l'oxygène pour un saut normal
    public void ConsumeOxygenForJump(float amount = -1)
    {
        if (amount < 0) amount = jumpConsumption;

        currentOxygen -= amount;
        Debug.Log("Saut! Oxygène consommé: " + amount + " | Oxygène restant: " + currentOxygen);

        if (currentOxygen <= 0)
        {
            currentOxygen = 0;
            Die();
        }

        if (oxygenBar != null)
        {
            oxygenBar.value = currentOxygen;
        }
    }

    // NOUVEAU - Consomme l'oxygène pour un wall jump
    public void ConsumeOxygenForWallJump()
    {
        ConsumeOxygenForJump(wallJumpConsumption);
        Debug.Log("Wall Jump! Oxygène consommé: " + wallJumpConsumption);
    }

    // NOUVEAU - Consomme l'oxygène pour un double jump
    public void ConsumeOxygenForDoubleJump()
    {
        ConsumeOxygenForJump(doubleJumpConsumption);
        Debug.Log("Double Jump! Oxygène consommé: " + doubleJumpConsumption);
    }

    // Retourne un ratio entre 0 et 1 pour le PlayerMovement
    public float GetOxygenRatio()
    {
        return currentOxygen / maxOxygen;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            isInWatter = true;
            AudioSource.PlayClipAtPoint(waterSound, transform.position);
            Debug.Log("Le kamtar est dans l'eau");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            isInWatter = false;
            Debug.Log("Le kamtar sort de l'eau");
        }
    }

    public void Die()
    {
        // Si on est déjà en train de mourir, on annule pour pas relancer l'animation en boucle
        if (isDying) return;

        isDying = true;
        Debug.Log("Lancement de l'animation de mort...");

        AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // 1. On coupe la physique pour que le joueur fige sur place (il ne tombe plus)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Désactive totalement les collisions et la gravité
        }

        // 2. On lance l'animation de mort
        if (anim != null)
        {
            anim.SetTrigger("isDead");
        }

        // 3. On lance le chrono avant de réapparaître !
        StartCoroutine(RespawnRoutine());
    }
    private System.Collections.IEnumerator RespawnRoutine()
    {
        // On dit au jeu d'attendre 1 seconde (modifie ce chiffre selon la durée de ton animation !)
        yield return new WaitForSeconds(1f);

        // --- LE TEMPS EST ÉCOULÉ, ON RESPAWN ---

        // On recharge l'oxygène
        currentOxygen = maxOxygen;
        if (oxygenBar != null) oxygenBar.value = maxOxygen;

        // On téléporte le joueur
        transform.position = respawnPosition;

        // On rallume la physique
        if (rb != null)
        {
            rb.simulated = true;
        }

        // On remet l'animation normale (retour à Idle)
        if (anim != null)
        {
            anim.SetTrigger("isRespawn");
        }

        // On autorise le joueur à mourir de nouveau
        isDying = false;
    }

    public void LoseOxygen(float damageAmount)
    {
        AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f);
        currentOxygen -= damageAmount;
        Debug.Log("Le kamtar a touché un ver, on perd " + damageAmount + " d'oxygène !");

        if (currentOxygen <= 0)
        {
            currentOxygen = 0;
            Die();
        }
    }

    // Appelée par l'étang quand le joueur le touche
    public void UpdateCheckpoint(Vector3 newCheckpointPosition)
    {
        respawnPosition = newCheckpointPosition;
        Debug.Log("Point de sauvegarde mis à jour !");
    }

    void RespawnPlayer()
    {
        // 1. On recharge l'oxygène à fond
        currentOxygen = maxOxygen;
        if (oxygenBar != null) oxygenBar.value = maxOxygen;

        // 2. On téléporte le joueur au dernier étang débloqué
        transform.position = respawnPosition;

        // 3. TRÈS IMPORTANT : On coupe la vitesse du joueur.
        // Sinon, s'il est mort en tombant super vite, il réapparaîtra à l'étang en tombant à la même vitesse !
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}