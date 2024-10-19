using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;

    public void SetHealth(float healthPercentage)
    {
        fillImage.fillAmount = healthPercentage;
    }
}