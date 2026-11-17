using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DefeatUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject defeatPanel;
    public TMP_Text killedByText;

    [Header("Buttons")]
    public Button tryAgainButton;
    public Button mainMenuButton;

    void Start()
    {
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowDefeat(string enemyName)
    {
        if (killedByText != null)
            killedByText.text = "Killed by " + enemyName;

        if (defeatPanel != null) 
            defeatPanel.SetActive(true);
    }

    void OnTryAgainClicked()
    {
        // I-restart ang laban (BattleScene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainMenuClicked()
    {
        // Bumalik sa Main Menu
        SceneManager.LoadScene("MainMenu"); 
    }
}