using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Color lowHealthColor = Color.red;
    public Color highHealthColor = Color.green;
    private Image fillImage;

    public void SetType(string type)
    {
        if (type == "castle")
        {
            lowHealthColor = Color.red;
            highHealthColor = Color.green;
        }
        else if (type == "monster")
        {
            lowHealthColor = Color.blue;
            highHealthColor = Color.yellow;
        }
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
    }

    private void Awake()
    {
        fillImage = slider.fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        // Update fill color based on health percentage
        float healthPercentage = slider.value / slider.maxValue;
        fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercentage);
    }
}
