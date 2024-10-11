using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Button muteButton;
    public AudioSource audioSource;
    public Image muteImage;

    private bool isMuted = false;

    void Start()
    {
        muteButton.onClick.AddListener(ToggleMute);
    }

    public void ToggleMute()
    {
        if (isMuted)
        {
            audioSource.volume = 0.8f;
            muteImage.color = ColorUtility.TryParseHtmlString("#A68208", out var newColor) ? newColor : Color.white;
            isMuted = false;
        }
        else
        {
            audioSource.volume = 0f;
            muteImage.color = ColorUtility.TryParseHtmlString("#7B7B7B", out var newColor) ? newColor : Color.white;
            isMuted = true;
        }
    }
}
