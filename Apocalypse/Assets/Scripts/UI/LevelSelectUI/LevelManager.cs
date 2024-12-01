using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private int clearedLevels = 1;

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

    public int GetClearedLevels()
    {
        return clearedLevels;
    }

    public void IncreaseClearedLevel()
    {
        clearedLevels++;
    }
}
