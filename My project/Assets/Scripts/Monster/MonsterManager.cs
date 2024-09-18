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
            new Monster("SpeedMonster1", speedMonster1Prefab, -0.04f, 1.5f),
            new Monster("SpeedMonster2", speedMonster2Prefab, 0, 1.5f),
            new Monster("SpeedMonster3", speedMonster3Prefab, 0, 2f),

            new Monster("StrengthMonster1", strengthMonster1Prefab, 0, 0.8f),
            new Monster("StrengthMonster2", strengthMonster2Prefab, 0, 0.7f),
            new Monster("StrengthMonster3", strengthMonster3Prefab, 0, 0.5f),

            new Monster("HealthMonster1", healthMonster1Prefab, 0, 0.6f),
            new Monster("HealthMonster2", healthMonster2Prefab, 0, 0.4f),
            new Monster("HealthMonster3", healthMonster3Prefab, 0, 0.25f),

            new Monster("InvisibleMonster1", invisibleMonster1Prefab, 1),
            new Monster("InvisibleMonster2", invisibleMonster2Prefab, 1),
            new Monster("InvisibleMonster3", invisibleMonster3Prefab, 1, 0.8f)
        };
    }

    // Method to find a monster by name
    public Monster GetMonsterByName(string monsterName)
    {
        return monsters.Find(monster => monster.name == monsterName);
    }
}