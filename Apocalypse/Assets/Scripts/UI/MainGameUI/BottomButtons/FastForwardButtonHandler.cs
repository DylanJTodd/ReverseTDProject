using UnityEngine;
using UnityEngine.UI;

public class FastForwardButtonHandler : MonoBehaviour
{
    public Button ffButton;
    public Image ffImage;
    public AudioSource audioSource;

    public bool isFF = false;

    private float originalPitch;

    void Start()
    {
        ffButton.onClick.AddListener(ToggleFF);
    }

    public void ToggleFF()
    {
        StatTracker statTracker = GameObject.Find("StatTracker").GetComponent<StatTracker>();
        statTracker.FastForwardToggle();
        if (isFF)
        {
            Time.timeScale = 1f;

            ffImage.color = ColorUtility.TryParseHtmlString("#A68208", out var newColor) ? newColor : Color.white;

            audioSource.volume = 0.8f;
            audioSource.pitch = originalPitch;

            isFF = false;

        }
        else
        {
            originalPitch = audioSource.pitch;
            Time.timeScale = 2f;

            ffImage.color = ColorUtility.TryParseHtmlString("#7B7B7B", out var newColor) ? newColor : Color.white;

            audioSource.volume = 0.8f;
            audioSource.pitch = originalPitch * 1.1f;

            isFF = true;
        }
    }
}
