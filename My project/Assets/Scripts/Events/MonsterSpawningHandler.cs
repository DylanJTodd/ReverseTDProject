using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawningHandler : MonoBehaviour
{
    // References to your buttons
    public Button speedSpawn;
    public Button strengthSpawn;
    public Button healthSpawn;
    public Button invisibleSpawn;

    public SpawningMonster spawningMonster;


    void Start()
    {
        speedSpawn.onClick.AddListener(() => OnButtonClick(1));
        strengthSpawn.onClick.AddListener(() => OnButtonClick(2));
        healthSpawn.onClick.AddListener(() => OnButtonClick(3));
        invisibleSpawn.onClick.AddListener(() => OnButtonClick(4));
    }

    void OnButtonClick(int buttonId)
    {
        switch (buttonId)
        {
            case 1:
                spawningMonster.SpawnMonster("SpeedMonster1");
                break;
            case 2:
                spawningMonster.SpawnMonster("StrengthMonster1");
                break;
            case 3:
                spawningMonster.SpawnMonster("HealthMonster1");
                break;
            case 4:
                spawningMonster.SpawnMonster("InvisibleMonster1");
                break;
            default:
                Debug.LogWarning("Unknown button ID");
                break;
        }
    }
}
