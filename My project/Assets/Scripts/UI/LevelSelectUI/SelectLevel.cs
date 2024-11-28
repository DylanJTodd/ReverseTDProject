using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectLevel : MonoBehaviour
{
    public static SelectLevel Instance { get; private set; }

    public Button[] levelButtons;

    // For class demo all levels are cleared
    private int clearedLevels = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            levelButtons[i].onClick.AddListener(() => LevelHandler(levelNumber));
        }
    }

    public void IncreaseClearedLevel()
    {
        clearedLevels++;
    }

    void LevelHandler(int levelNumber)
    {
        Debug.Log(clearedLevels);
        Debug.Log(levelNumber);
        if (levelNumber > clearedLevels)
        {
            return;
        }
        SceneManager.LoadScene($"Level{levelNumber}");
    }
}
