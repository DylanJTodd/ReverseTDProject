using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroButtonHandler : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    public CanvasGroup fadePanel;
    private AudioSource audioSource;
    public float fadeDuration = 1f;

    public AudioClip menuMusic;

    private Color normalColor;
    public Color hoverColor = Color.yellow;

    private bool isCoroutineRunning = false;

    void Start()
    {
        fadePanel.alpha = 0;
        normalColor = playButton.GetComponent<Image>().color;
        playButton.onClick.AddListener(OnPlayButtonClick);
        quitButton.onClick.AddListener(OnQuitButtonClick);
        AddHoverEffects(playButton);
        AddHoverEffects(quitButton);

        GameObject audioObject = GameObject.Find("PersistentAudio");
        if (audioObject == null)
        {
            audioObject = new GameObject("PersistentAudio");
            DontDestroyOnLoad(audioObject);

            audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.8f;
            audioSource.pitch = 1f;
            audioSource.Play();
        }
        else
        {
            GameObject audioSourceObject = GameObject.Find("Audio Source");
            if (audioSourceObject != null)
            {
                Destroy(audioSourceObject);
            }

            audioSource = audioObject.GetComponent<AudioSource>();
            audioSource = audioObject.GetComponent<AudioSource>();
            audioSource.pitch = 1f;
        }
        StartCoroutine(OpenScene());
    }

    public void Update()
    {
        if (fadePanel.alpha != 0)
        {
            if (isCoroutineRunning == false)
            {
                StartCoroutine(OpenScene());
            }
        }
    }

    private IEnumerator OpenScene()
    {
        isCoroutineRunning = true;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += 0.01f;
            fadePanel.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));

            if (audioSource != null)
            {
                audioSource.volume = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeDuration);
            }

            yield return null;
        }
        fadePanel.alpha = 0f;
        isCoroutineRunning = false;
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
        isCoroutineRunning = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += 0.01f;
            fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            yield return null;
        }

        fadePanel.alpha = 1f;
        SceneManager.LoadScene("LevelSelect");
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
