using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { 
        get; 
        private set; 
    }

    [SerializeField] private CanvasGroup panel;
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private int mainMenuSceneIndex = 0;

    [Header("Keys")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
    [SerializeField] private KeyCode altToggleKey = KeyCode.Escape;
    [SerializeField] private KeyCode mainMenuKey = KeyCode.M;

    public bool IsPaused { 
        get; 
        private set; 
    }

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (panel == null)
        {
            Debug.LogError("PauseMenu: Panel (CanvasGroup) is not assigned.", this);
            return;
        }

        SetPanelInstant(false);
    }

    private void Update()
    {
        if (IsOnMainMenu())
        {
            return;
        }

        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(altToggleKey))
        {
            Toggle();
        }
        else if (IsPaused && Input.GetKeyDown(mainMenuKey))
        {
            ReturnToMainMenu();
        }
    }

    private bool IsOnMainMenu()
    {
        return SceneManager.GetActiveScene().buildIndex == mainMenuSceneIndex;
    }

    public void Toggle()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        Fade(1f);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Fade(0f);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SetPanelInstant(false);
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void Fade(float target)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = panel.alpha;
        float t = 0f;

        if (target > 0f)
        {
            panel.gameObject.SetActive(true);
        }

        while (t < fadeDuration)
        {
            // deltaTime is 0 while paused
            t += Time.unscaledDeltaTime;
            panel.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        panel.alpha = target;
        panel.interactable = false;
        panel.blocksRaycasts = target > 0f;

        if (target == 0f)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void SetPanelInstant(bool visible)
    {
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = false;
        panel.blocksRaycasts = visible;
        panel.gameObject.SetActive(visible);
    }
}