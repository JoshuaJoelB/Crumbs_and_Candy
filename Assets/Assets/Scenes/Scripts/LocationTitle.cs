using UnityEngine;
using TMPro;
using System.Collections;

public class LocationTitle : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text locationText;
    public CanvasGroup canvasGroup;

    [Header("Location Settings")]
    public string locationName = "Blacksmith";

    [Header("Fade Settings")]
    public float displayDuration = 2f;
    public float fadeDuration = 1.5f;

    private void Start()
    {
        locationText.text = locationName;
        canvasGroup.alpha = 1f;
        StartCoroutine(FadeLocationTitle());
    }

    private IEnumerator FadeLocationTitle()
    {
        // Wait before fading
        yield return new WaitForSeconds(displayDuration);
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
