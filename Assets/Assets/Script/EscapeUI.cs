using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EscapeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject escapePanel;
    public TMP_Text titleText;       // "ESCAPE"
    public TMP_Text descriptionText; // Yung description
    
    [Header("Buttons")]
    public Button playAgainButton;   // Babalik sa Main Menu
    public Button quitButton;        // Magsasara ng laro

    void Start()
    {
        if (escapePanel != null) escapePanel.SetActive(false);

        // I-setup ang buttons
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    // Tatawagin natin ito pagkatapos matalo si Witch
    public void ShowEscapeScreen()
    {
        if (titleText != null) 
            titleText.text = "ESCAPE";

        if (descriptionText != null)
            descriptionText.text = "Successfully escape!\nYou successfully defeat the witch and find your way out to the forest.";

        if (escapePanel != null) 
            escapePanel.SetActive(true);
        
        // Siguraduhing kita ang mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnPlayAgainClicked()
    {
        // Babalik sa Main Menu
        SceneManager.LoadScene("MainMenu");
    }

    void OnQuitClicked()
    {
        // Magsasara ang laro
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}