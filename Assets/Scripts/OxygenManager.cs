using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OxygenManager : MonoBehaviour
{
    [Header("Parametres oxygene")]
    public float maxOxygen = 10f;
    private float currentOxygen;
    private bool isInWatter = false;

    [Header("Vitesse de recharge de l'oxygene")]
    public float rechargeSpeed = 5f;

    [Header("Interface Graphique UI")]
    public Slider oxygenBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        if (oxygenBar != null)
        {
            oxygenBar.minValue = 0f;
            oxygenBar.maxValue = maxOxygen;
            oxygenBar.value = maxOxygen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // si on est pas dans l'eau -> ca baisse
        if (!isInWatter)
        {
            currentOxygen -= Time.deltaTime;

            Debug.Log("Oxygene actuel : " + currentOxygen);

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

            if(currentOxygen > maxOxygen)
            {
                currentOxygen = maxOxygen;
            }
        }
        if(oxygenBar != null)
        {
            oxygenBar.value = currentOxygen;
        }
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

    private void Die()
    {
        Debug.Log("Le kamtar a plus d'air");
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
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
}
