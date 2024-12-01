using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public Button pauseButton;
    public Image pauseImage;
    public AudioSource audioSource;
    public Image pausePanel;
    public FastForwardButtonHandler fastForwardButtonHandler;

    private Button retryButton;
    private Button MainMenu;

    private CanvasGroup loadingFadePanel;
    private CanvasGroup menuButtons;

    private bool isPaused = false;
    private float originalPitch;

    void Start()
    {
        menuButtons = GameObject.Find("MenuButtons").GetComponentInChildren<CanvasGroup>();
        loadingFadePanel = GameObject.Find("BlackFade").GetComponentInChildren<CanvasGroup>();

        retryButton = GameObject.Find("RetryButton").GetComponentInChildren<Button>();
        MainMenu = GameObject.Find("MainMenuButton").GetComponentInChildren<Button>();

        retryButton.onClick.AddListener(RetryLevel);
        MainMenu.onClick.AddListener(ToMainMenu);
        pauseButton.onClick.AddListener(TogglePause);

        menuButtons.alpha = 0;
        menuButtons.blocksRaycasts = false;
        menuButtons.interactable = false;
    }

    private void RetryLevel()
    {
        loadingFadePanel.alpha = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        loadingFadePanel.alpha = 0;
    }
    private void ToMainMenu()
    {
        loadingFadePanel.alpha = 1;
        SceneManager.LoadScene("LevelSelect");
    }

    public void TogglePause()
    {
        StatTracker statTracker = GameObject.Find("StatTracker").GetComponent<StatTracker>();
        statTracker.PauseToggle();
        if (isPaused)
        {
            if (fastForwardButtonHandler.isFF)
            {
                Time.timeScale = 2f;
            }
            else
            {
                Time.timeScale = 1f;
            }

            audioSource.pitch = originalPitch;

            pauseImage.color = ColorUtility.TryParseHtmlString("#A68208", out var newColor) ? newColor : Color.white;

            pausePanel.fillAmount = 0f;
            menuButtons.alpha = 0;
            menuButtons.interactable = false;
            menuButtons.blocksRaycasts = false;
            pausePanel.raycastTarget = false;

            isPaused = false;
        }
        else
        {
            originalPitch = audioSource.pitch;
            Time.timeScale = 0f;

            pauseImage.color = ColorUtility.TryParseHtmlString("#7B7B7B", out var newColor) ? newColor : Color.white;

            audioSource.volume = 0.5f;
            audioSource.pitch = 0.6f;

            pausePanel.fillAmount = 1f;
            menuButtons.alpha = 1;
            menuButtons.interactable = true;
            menuButtons.blocksRaycasts = true;
            pausePanel.raycastTarget = true;

            isPaused = true;
        }
    }
}
