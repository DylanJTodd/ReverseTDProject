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
        
        StartCoroutine(LoadNextLevel());
    }

    public void GameLost()
    {
        gameOverScreen.alpha = 1;
        gameLost.alpha = 1;
    }

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(5f);

        // Get current level number from scene name
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName.StartsWith("Level"))
        {
            string levelNumberStr = currentSceneName.Substring(5);
            if (int.TryParse(levelNumberStr, out int currentLevel))
            {
                int nextLevel = currentLevel + 1;
                string nextSceneName = $"Level{nextLevel}";

                // Check if next level scene exists
                if (Application.CanStreamedLevelBeLoaded(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    // If no more levels, return to level select
                    SceneManager.LoadScene("LevelSelect");
                }
            }
        }
    }
}
