using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOver : MonoBehaviour
{
    public CanvasGroup gameOverScreen;
    public CanvasGroup gameWon;
    public CanvasGroup gameLost;

    public void GameWon()
    {
        gameOverScreen.alpha = 1;
        gameWon.alpha = 1;
        
        StartCoroutine(LoadLevelSelect());
    }

    public void GameLost()
    {
        gameOverScreen.alpha = 1;
        gameLost.alpha = 1;
    }

    private IEnumerator LoadLevelSelect()
    {
        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene("LevelSelect");
    }
}
