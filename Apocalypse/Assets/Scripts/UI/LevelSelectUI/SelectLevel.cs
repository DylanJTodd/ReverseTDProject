using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectLevel : MonoBehaviour
{
    public Button[] levelButtons;
    public Button mainMenuButton;

    public CanvasGroup fadePanel;
    private AudioSource audioSource;

    public AudioClip menuMusic;

    public CanvasGroup sceneFade;

    private bool isCoroutineRunning = false;

    public float fadeDuration = 0.5f;

    void Start()
    {
        sceneFade.alpha = 0;
        fadePanel.alpha = 1;
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
            audioSource.pitch = 1.5f;
            audioSource.Play();
        }
        else
        {
            audioSource = audioObject.GetComponent<AudioSource>();
            if(audioSource == null)
            {
                audioSource = audioObject.AddComponent<AudioSource>();
                audioSource.clip = menuMusic;
                audioSource.loop = true;
                audioSource.playOnAwake = false;
                audioSource.volume = 0.8f;
                audioSource.pitch = 1.5f;
                audioSource.Play();
            }
        }

        mainMenuButton.onClick.AddListener(() => StartCoroutine(ToMainMenu()));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            levelButtons[i].onClick.AddListener(() => LevelHandler(levelNumber));
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

    private IEnumerator ToMainMenu()
    {
        isCoroutineRunning = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += 0.01f;
            fadePanel.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (audioSource != null)
            {
                audioSource.volume = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            }

            yield return null;
        }

        fadePanel.alpha = 1f;
        isCoroutineRunning = false;
        SceneManager.LoadScene("IntroScreen");
    }

    void LevelHandler(int levelNumber)
    {
        /*    if (levelNumber <= LevelManager.Instance.GetClearedLevels())
            {
                sceneFade.alpha = 1;
                GameObject audioObject = GameObject.Find("PersistentAudio");
                Destroy(audioObject);

                SceneManager.LoadScene($"Level{levelNumber}");
            }
        */
        //The above section is if we wanted to force the user to complete the previous levels in order to move on

        sceneFade.alpha = 1;
        GameObject audioObject = GameObject.Find("PersistentAudio");
        Destroy(audioObject);

        SceneManager.LoadScene($"Level{levelNumber}");
    }
}
