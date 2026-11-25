using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EscapeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject escapePanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    
    [Header("Buttons")]
    public Button playAgainButton;
    public Button quitButton;

    void Start()
    {
        if (escapePanel != null) escapePanel.SetActive(false);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
        
        Debug.Log("[EscapeUI] Script Started and Ready.");
    }

    public void ShowEscapeScreen()
    {
        Debug.Log("[EscapeUI] ShowEscapeScreen() was called!");
        
        if (titleText != null) titleText.text = "ESCAPE";
        if (descriptionText != null) descriptionText.text = "Successfully escape!\nYou successfully defeat the witch and find your way out to the forest.";
        
        if (escapePanel != null) 
        {
            escapePanel.SetActive(true);
            Debug.Log("[EscapeUI] Escape Panel is now ACTIVE.");
        }
        else
        {
            Debug.LogError("[EscapeUI] ERROR: escapePanel is NULL! Check the Inspector.");
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnPlayAgainClicked() 
    { 
        Debug.Log("[EscapeUI] Play Again Clicked. Loading Main Menu...");
        SceneManager.LoadScene("MainMenu"); 
    }
    
    void OnQuitClicked() 
    { 
        Debug.Log("[EscapeUI] Quit Clicked. Quitting Game...");
        Application.Quit(); 
    }
}