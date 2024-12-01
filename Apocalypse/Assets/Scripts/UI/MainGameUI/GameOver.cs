using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public CanvasGroup gameOverScreen;
    public CanvasGroup gameWon;
    public CanvasGroup gameLost;

    public Button nextLevelButton;
    public Button menuButtonWon;
    public Button menuButtonLost;
    public Button retryButtonWon;
    public Button retryButtonLost;

    private CanvasGroup fadePanel;
    private Boolean statsDisplayed = false;

    private bool calledLevelManager = false;
    public void Start()
    {
        retryButtonWon.onClick.AddListener(RestartScene);
        retryButtonLost.onClick.AddListener(RestartScene);
        menuButtonWon.onClick.AddListener(ToMenu);
        menuButtonLost.onClick.AddListener(ToMenu);
        nextLevelButton.onClick.AddListener(NextScene);

        fadePanel = GameObject.Find("BlackFade").GetComponent<CanvasGroup>();
        gameOverScreen.alpha = 0;
        gameOverScreen.blocksRaycasts = false;
        gameOverScreen.interactable = false;

    }

    private void RestartScene()
    {
        fadePanel.alpha = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void NextScene()
    {
        fadePanel.alpha = 1;
        //(old code, not for demo)SceneManager.LoadScene($"Level{LevelManager.Instance.GetClearedLevels()}");
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Level10")
        {
            SceneManager.LoadScene("Level1");
        }
        string numberString = System.Text.RegularExpressions.Regex.Match(currentSceneName, @"\d+$").Value;

        if (int.TryParse(numberString, out int currentLevel))
        {
            int nextLevel = currentLevel + 1;
            string nextSceneName = "Level" + nextLevel;
            Debug.Log($"nextSceneName: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("Level1");
        }
    }
    private void ToMenu()
    {
        fadePanel.alpha = 1;
        SceneManager.LoadScene("LevelSelect");
    }
    public void GameWon()
    {
        gameOverScreen.alpha = 1;
        gameOverScreen.blocksRaycasts = true;
        gameOverScreen.interactable = true;
        gameWon.blocksRaycasts = true;
        gameWon.interactable = true;
        gameLost.blocksRaycasts = false;
        gameLost.interactable = false;
        gameWon.alpha = 1;

        if (!calledLevelManager)
        {
            calledLevelManager = true;
            LevelManager.Instance.IncreaseClearedLevel();
        }
        if (!statsDisplayed)
        {
            statsDisplayed = true;
            DisplayStats(true);
        }
    }

    public void GameLost()
    {
        gameOverScreen.alpha = 1;
        gameOverScreen.blocksRaycasts = true;
        gameOverScreen.interactable = true;
        gameLost.blocksRaycasts = true;
        gameLost.interactable = true;
        gameWon.blocksRaycasts = false;
        gameWon.interactable = false;
        gameLost.alpha = 1;

        if (!statsDisplayed)
        {
            statsDisplayed = true;
            DisplayStats(false);
        }
    }

    public void DisplayStats(bool winOrLoss)
    {
        Time.timeScale = 0f;
        TextMeshProUGUI statsText;
        StatTracker statTracker = GameObject.Find("StatTracker").GetComponent<StatTracker>();

        if (winOrLoss)
        {
            statsText = GameObject.Find("StatsWon").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            statsText = GameObject.Find("StatsLost").GetComponent<TextMeshProUGUI>();
        }

        List<string> stats = statTracker.ReturnStats();

        statsText.text = string.Format(
            "Money Gained: ${0}\n" +
            "Money Spent: ${1}\n" +
            "Highest Money Held: ${2}\n" +
            "Average Spending: ${3}\n" +
            "Waves Played: {4}\n" +
            "Longest Wave: {5}s\n" +
            "Shortest Wave: {6}s\n" +
            "Average Wave Length: {7}s\n" +
            "Total Time: {8}s",
            stats[0], stats[1], stats[2], stats[3],
            stats[4], stats[5], stats[6], stats[7], stats[8]
        );
    }
}
