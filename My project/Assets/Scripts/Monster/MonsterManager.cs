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

    public Dictionary<string, GameObject> monsters;

    void Awake()
    {
        monsters = new Dictionary<string, GameObject>
        {
            {"SpeedMonster1", speedMonster1Prefab },
            {"SpeedMonster2", speedMonster2Prefab },
            {"SpeedMonster3", speedMonster3Prefab },

            {"StrengthMonster1", strengthMonster1Prefab },
            {"StrengthMonster2", strengthMonster2Prefab },
            {"StrengthMonster3", strengthMonster3Prefab },

            {"HealthMonster1", healthMonster1Prefab },
            {"HealthMonster2", healthMonster2Prefab },
            {"HealthMonster3", healthMonster3Prefab },

            {"InvisibleMonster1", invisibleMonster1Prefab },
            {"InvisibleMonster2", invisibleMonster2Prefab },
            {"InvisibleMonster3", invisibleMonster3Prefab }
        };
    }

    public GameObject GetMonsterByName(string monsterName)
    {
        return monsters[monsterName];
    }
}