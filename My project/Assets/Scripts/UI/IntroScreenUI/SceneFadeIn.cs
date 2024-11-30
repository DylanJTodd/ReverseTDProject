using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public AudioSource audioSource;
    public float fadeDuration = 15f;

    void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        fadePanel.alpha = 1f;

        audioSource.volume = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += 0.01f;
            yield return null;
        }

        fadePanel.alpha = 0;
        audioSource.volume = 0.8f;
    }
}
