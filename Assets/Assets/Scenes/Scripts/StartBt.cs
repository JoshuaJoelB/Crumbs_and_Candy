using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBt : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settings;

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settings.SetActive(true);
    }

    public void CloseSettings()
    {
        settings.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();

        //Debug.Log("Game Quit");
    }
}