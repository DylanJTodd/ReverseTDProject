using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    public GameObject speedMonster1Prefab;
    public GameObject speedMonster2Prefab;
    public GameObject speedMonster3Prefab;

    public GameObject strengthMonster1Prefab;
    public GameObject strengthMonster2Prefab;
    public GameObject strengthMonster3Prefab;

    public GameObject healthMonster1Prefab;
    public GameObject healthMonster2Prefab;
    public GameObject healthMonster3Prefab;

    public GameObject invisibleMonster1Prefab;
    public GameObject invisibleMonster2Prefab;
    public GameObject invisibleMonster3Prefab;

    public List<Monster> monsters;

    void Awake()
    {
        monsters = new List<Monster>
        {
            new Monster("SpeedMonster1", speedMonster1Prefab, 25, 400, 100, -0.04f, 1.5f),
            new Monster("SpeedMonster2", speedMonster2Prefab, 75, 1000, 250, 0, 1.8f),
            new Monster("SpeedMonster3", speedMonster3Prefab, 250, 10000, 950, 0, 1.5f),

            new Monster("StrengthMonster1", strengthMonster1Prefab, 20, 500, 250, 0, 1f),
            new Monster("StrengthMonster2", strengthMonster2Prefab, 100, 3000, 950, 0, 0.7f),
            new Monster("StrengthMonster3", strengthMonster3Prefab, 300, 25000, 5000, 0, 0.7f),

            new Monster("HealthMonster1", healthMonster1Prefab, 50, 2500, 200, 0, 0.7f),
            new Monster("HealthMonster2", healthMonster2Prefab, 150, 2500, 200, 0, 0.7f),
            new Monster("HealthMonster3", healthMonster3Prefab, 400, 2500, 200, 0, 0.4f),

            new Monster("InvisibleMonster1", invisibleMonster1Prefab,30, 200, 50, 1, 1f),
            new Monster("InvisibleMonster2", invisibleMonster2Prefab, 100, 750, 250, 1, 1f),
            new Monster("InvisibleMonster3", invisibleMonster3Prefab,400, 1, 15000, 2000, 0.7f)
        };
    }

    // Method to find a monster by name
    public Monster GetMonsterByName(string monsterName)
    {
        return monsters.Find(monster => monster.name == monsterName);
    }
}