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

    [Header("Interface Graphique UI")]
    public Slider oxygenBar;

    public Vector3 respawnPosition;
    private Rigidbody2D rb;

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
        Debug.Log("Le kamtar a plus d'air");
        RespawnPlayer();
    }

    public void LoseOxygen(float damageAmount)
    {
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
            // Note pour Unity 6 : on utilise linearVelocity au lieu de velocity
            rb.linearVelocity = Vector2.zero;
        }
    }
}