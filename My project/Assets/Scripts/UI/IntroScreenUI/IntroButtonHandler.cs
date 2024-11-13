using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroButtonHandler : MonoBehaviour
{
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    public CanvasGroup fadePanel;
    public AudioSource audioSource;
    public float fadeDuration = 0.5f;

    private Color normalColor;
    public Color hoverColor = Color.yellow;

    void Start()
    {
        normalColor = playButton.GetComponent<Image>().color;

        playButton.onClick.AddListener(OnPlayButtonClick);
        settingsButton.onClick.AddListener(OnSettingsButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);

        AddHoverEffects(playButton);
        AddHoverEffects(settingsButton);
        AddHoverEffects(quitButton);
    }

    private void AddHoverEffects(Button button)
    {
        var eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entry1 = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry1.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry1.callback.AddListener((eventData) => { OnPointerEnter(button); });
        eventTrigger.triggers.Add(entry1);

        var entry2 = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry2.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entry2.callback.AddListener((eventData) => { OnPointerExit(button); });
        eventTrigger.triggers.Add(entry2);
    }

    private void OnPlayButtonClick()
    {
        StartCoroutine(FadeOutAndStartGame());
    }

    private IEnumerator FadeOutAndStartGame()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (audioSource != null)
            {
                audioSource.volume = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            }

            yield return null;
        }

        fadePanel.alpha = 1f;
        if (audioSource != null)
        {
            audioSource.volume = 0f;
        }

        // Now load the next scene
        SceneManager.LoadScene("LevelSelect");
    }

    private void OnSettingsButtonClick()
    {
        Debug.Log("Settings Button Clicked");
    }

    private void OnQuitButtonClick()
    {
        Application.Quit();
        Debug.Log("Quit Button Clicked");
    }

    private void OnPointerEnter(Button button)
    {
        button.GetComponent<Image>().color = hoverColor;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Bold;
        }
    }

    private void OnPointerExit(Button button)
    {
        button.GetComponent<Image>().color = normalColor;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Normal;
        }
    }
}