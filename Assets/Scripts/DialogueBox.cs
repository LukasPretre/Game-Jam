using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    // NOUVEAU - Tableau d'images dans le même ordre que les textes
    public Image[] characterImages;

    private int index;

    void Awake()
    {
        textComponent.text = string.Empty;

        // MODIFIÉ - vérifie les images assignées
        if (characterImages != null)
        {
            Debug.Log("Nombre d'images assignées: " + characterImages.Length);
            foreach (Image img in characterImages)
            {
                if (img != null)
                {
                    Debug.Log("Image trouvée: " + img.name + " | Active: " + img.gameObject.activeSelf);
                    img.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogError("characterImages est NULL!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void StartDialoguePublic()
    {
        gameObject.SetActive(true); // Affiche le dialogue
        index = 0;
        textComponent.text = string.Empty;

        // NOUVEAU - Affiche la première image
        UpdateCharacterImage();

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;

            // NOUVEAU - Change l'image
            UpdateCharacterImage();

            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // NOUVEAU - Affiche l'image correspondante et cache les autres
    void UpdateCharacterImage()
    {
        Debug.Log("UpdateCharacterImage appelé - Index: " + index);

        if (characterImages == null || characterImages.Length == 0)
        {
            Debug.LogError("characterImages est vide ou null!");
            return;
        }

        Debug.Log("Nombre d'images dans le tableau: " + characterImages.Length);

        // Cache toutes les images
        foreach (Image img in characterImages)
        {
            if (img != null)
            {
                img.gameObject.SetActive(false);
                Debug.Log("Image cachée: " + img.name);
            }
        }

        // Affiche l'image correspondante à l'index
        if (index < characterImages.Length && characterImages[index] != null)
        {
            characterImages[index].gameObject.SetActive(true);
            Debug.Log("Image affichée: " + characterImages[index].name + " à l'index " + index);
        }
        else
        {
            Debug.LogError("Index " + index + " out of range! Nombre d'images: " + characterImages.Length);
        }
    }
}