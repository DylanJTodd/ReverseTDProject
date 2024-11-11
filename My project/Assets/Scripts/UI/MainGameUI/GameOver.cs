using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public CanvasGroup gameOverScreen;
    public CanvasGroup gameWon;
    public CanvasGroup gameLost;
    public void GameWon()
    {
        gameOverScreen.alpha = 1;
        gameWon.alpha = 1;

        //electLevel.Instance.IncreaseClearedLevel();
    }

    public void GameLost()
    {
        gameOverScreen.alpha = 1;
        gameLost.alpha = 1;
    }
}
