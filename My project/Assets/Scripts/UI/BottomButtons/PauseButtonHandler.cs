using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public Button pauseButton;
    public Image pauseImage;
    public AudioSource audioSource;
    public Image pausePanel;
    public FastForwardButtonHandler fastForwardButtonHandler;

    private bool isPaused = false;

    void Start()
    {
        pauseButton.onClick.AddListener(TogglePause);
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            if (fastForwardButtonHandler.isFF)
            {
                Time.timeScale = 2f;

                audioSource.volume = 1f;
                audioSource.pitch = 1f;
            }
            else
            {
                Time.timeScale = 1f;

                audioSource.volume = 0.8f;
                audioSource.pitch = 0.8f;
            }

            pauseImage.color = ColorUtility.TryParseHtmlString("#A68208", out var newColor) ? newColor : Color.white;

            pausePanel.fillAmount = 0f;
            pausePanel.raycastTarget = false;

            isPaused = false;
        }
        else
        {
            Time.timeScale = 0f;

            pauseImage.color = ColorUtility.TryParseHtmlString("#7B7B7B", out var newColor) ? newColor : Color.white;

            audioSource.volume = 0.5f;
            audioSource.pitch = 0.6f;

            pausePanel.fillAmount = 1f;
            pausePanel.raycastTarget = true;

            isPaused = true;
        }
    }
}
