using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public CanvasGroup gameOverScreen;
    public Image gameWon;
    public Image gameLost;
    public void GameWon()
    {
        gameOverScreen.alpha = 1;
        gameWon.fillAmount = 1;
    }

    public void GameLost()
    {
        gameOverScreen.alpha = 0;
        gameWon.fillAmount = 0;
    }
}
