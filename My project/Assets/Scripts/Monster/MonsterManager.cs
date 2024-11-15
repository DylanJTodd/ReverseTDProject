using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class MonsterManager : MonoBehaviour
{
    [Header("Speed Monster Prefabs")]
    public GameObject speedMonster1Prefab;
    public GameObject speedMonster2Prefab;
    public GameObject speedMonster3Prefab;

    [Header("Strength Monster Prefabs")]
    public GameObject strengthMonster1Prefab;
    public GameObject strengthMonster2Prefab;
    public GameObject strengthMonster3Prefab;

    [Header("Health Monster Prefabs")]
    public GameObject healthMonster1Prefab;
    public GameObject healthMonster2Prefab;
    public GameObject healthMonster3Prefab;

    [Header("Ethernal Monster Prefabs")]
    public GameObject invisibleMonster1Prefab;
    public GameObject invisibleMonster2Prefab;
    public GameObject invisibleMonster3Prefab;


    public static MonsterManager instance;
    public List<Monster> monsters = new List<Monster>();

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

    public void RegisterMonster(Monster monster)
    {
        if (!monsters.Contains(monster))
        {
            monsters.Add(monster);
        }
    }

    public void UnregisterMonster(Monster monster)
    {
        if (monsters.Contains(monster))
        {
            monsters.Remove(monster);
        }
    }

    public List<Monster> GetMonstersInRadius(Vector3 position, int radius)
    {
        return monsters.Where(monster => Vector3.Distance(monster.transform.position, position) <= radius).ToList();
    }

    public GameObject GetMonsterByName(string name)
    {
        return monsters.FirstOrDefault(monster => monster.name == name)?.gameObject;
    }


    public GameObject GetMonsterPrefab(string monsterName)
    {
        GameObject prefab = null;
        
        switch (monsterName)
        {
            case "StrengthMonster1": prefab = strengthMonster1Prefab; break;
            case "StrengthMonster2": prefab = strengthMonster2Prefab; break;
            case "StrengthMonster3": prefab = strengthMonster3Prefab; break;
            case "SpeedMonster1": prefab = speedMonster1Prefab; break;
            case "SpeedMonster2": prefab = speedMonster2Prefab; break;
            case "SpeedMonster3": prefab = speedMonster3Prefab; break;
            case "HealthMonster1": prefab = healthMonster1Prefab; break;
            case "HealthMonster2": prefab = healthMonster2Prefab; break;
            case "HealthMonster3": prefab = healthMonster3Prefab; break;
            case "InvisibleMonster1": prefab = invisibleMonster1Prefab; break;
            case "InvisibleMonster2": prefab = invisibleMonster2Prefab; break;
            case "InvisibleMonster3": prefab = invisibleMonster3Prefab; break;
            default:
                Debug.LogError($"Monster prefab with name {monsterName} not found");
                return null;
        }

        if (prefab == null)
        {
            Debug.LogError($"Prefab for monster {monsterName} is not assigned in the inspector");
            return null;
        }

        return prefab;
    }

    public GameObject CreateNewMonster(string name)
    {
        GameObject prefab = GetMonsterPrefab(name);
        if (prefab == null)
        {
            Debug.LogError($"Failed to get monster prefab: {name}");
            return null;
        }

        try
        {
            GameObject monsterObject = Instantiate(prefab);
            monsterObject.name = name;

            // Add the appropriate Monster component based on the name
            Monster monster = null;
            if (name.Contains("Strength"))
            {
                monster = monsterObject.AddComponent<StrengthMonster>();
            }
            else if (name.Contains("Speed"))
            {
                monster = monsterObject.AddComponent<SpeedMonster>();
            }
            else if (name.Contains("Health"))
            {
                monster = monsterObject.AddComponent<HealthMonster>();
            }
            else if (name.Contains("Invisible"))
            {
                monster = monsterObject.AddComponent<EthernalMonster>();
            }

            if (monster != null)
            {
                RegisterMonster(monster);
                return monsterObject;
            }
            else
            {
                Debug.LogError($"Failed to add Monster component to {name}");
                Destroy(monsterObject);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating monster {name}: {e.Message}");
            return null;
        }
    }

    public List<Monster> GetMonsters()
    {
        return monsters;
    }
}
