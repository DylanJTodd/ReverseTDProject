using UnityEngine;
using UnityEngine.UI;

public class FastForwardButtonHandler : MonoBehaviour
{
    public Button ffButton;
    public Image ffImage;
    public AudioSource audioSource;

    public bool isFF = false;

    void Start()
    {
        ffButton.onClick.AddListener(ToggleFF);
    }

    public void ToggleFF()
    {
        if (isFF)
        {
            Time.timeScale = 1f;

            ffImage.color = ColorUtility.TryParseHtmlString("#A68208", out var newColor) ? newColor : Color.white;

            audioSource.volume = 0.8f;
            audioSource.pitch = 0.8f;

            isFF = false;
        }
        else
        {
            Time.timeScale = 2f;

            ffImage.color = ColorUtility.TryParseHtmlString("#7B7B7B", out var newColor) ? newColor : Color.white;

            audioSource.volume = 1f;
            audioSource.pitch = 1f;

            isFF = true;
        }
    }
}
