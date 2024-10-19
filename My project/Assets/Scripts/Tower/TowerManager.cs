using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;
    public List<BaseTower> towers = new List<BaseTower>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Optional: Persist across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterTower(BaseTower tower)
    {
        if (!towers.Contains(tower))
        {
            towers.Add(tower);
        }
    }

    public void UnregisterTower(BaseTower tower)
    {
        if (towers.Contains(tower))
        {
            towers.Remove(tower);
        }
    }

    // Additional management methods can be added here
}
