using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
    [Header("Glisse ton Panel_EcranFin ici")]
    public GameObject endScreenPanel;

    void Update()
    {
        // La touche magique de secours (F12)
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TriggerEndScreen();
        }
    }

    // Cette fonction affiche l'écran et fige le jeu
    public void TriggerEndScreen()
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // Cette fonction sera appelée par ton bouton
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}