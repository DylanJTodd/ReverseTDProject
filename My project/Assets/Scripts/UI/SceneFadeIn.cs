using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public AudioSource audioSource;
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        fadePanel.alpha = 1f;

        audioSource.volume = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            fadePanel.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));

            audioSource.volume = Mathf.Clamp01(elapsedTime / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 0f;
        audioSource.volume = 1f;
    }
}
