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

        // Reset all game states
        Time.timeScale = 1f;

        // Find and destroy the PauseManager
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            Destroy(pauseManager.gameObject);
        }
        
        // Find and reset any other persistent managers that might affect the level select
        // For example, if you have a SelectLevel manager:
        SelectLevel selectLevel = FindObjectOfType<SelectLevel>();
        if (selectLevel != null)
        {
            Destroy(selectLevel.gameObject);
        }
        
        SceneManager.LoadScene("LevelSelect");
    }
}
