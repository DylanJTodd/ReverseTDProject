using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CastleHealth : MonoBehaviour
{
    public Image healthBar;
    public TextMeshProUGUI text;
    public GameOver gameOverScreen;

    public int maxCastleHealth = 1000;
    private int castleHealth;

    public void Start()
    {
        castleHealth = maxCastleHealth;
        text.text = $"{castleHealth} / {maxCastleHealth}";
    }

    public void RemoveHealth(int amount)
    {
        castleHealth -= amount;
        float fillAmount = Mathf.Max((float)castleHealth / (float)maxCastleHealth, 0.0f);
        healthBar.fillAmount = fillAmount;

        text.text = $"{castleHealth} / {maxCastleHealth}";

        if (castleHealth <= 0)
        {
            gameOverScreen.GameWon();
        }
    }
}
