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

    public void Update()
    {
        CleanupDestroyedMonsters();
    }

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
        if (monster == null)
        {
            Debug.LogWarning("Attempted to unregister null monster");
            return;
        }

        // Clean up any null entries first
        monsters.RemoveAll(m => m == null);
        
        // Then remove the specific monster if it exists
        if (monsters.Contains(monster))
        {
            monsters.Remove(monster);
        }
    }

    public List<Monster> GetMonstersInRadius(Vector3 position, float radius)
    {
        Debug.DrawLine(position, position + Vector3.up * radius, Color.red, 1f);
        return monsters.Where(monster => Vector3.Distance(monster.transform.position, position) <= radius).ToList();
    }

    public void ApplyDamageToMonstersInRadius(Vector3 position, float radius, int damage)
    {
        foreach (var monster in GetMonstersInRadius(position, radius))
        {
            monster.TakeDamage(damage);
        }
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

    public void ReplaceMonster(GameObject currentMonster, MonsterType type, int tier)
    {
        if (currentMonster == null)
        {
            Debug.LogError("Attempted to replace null monster");
            return;
        }

        // Store the current position and rotation
        Vector3 position = currentMonster.transform.position;
        Quaternion rotation = currentMonster.transform.rotation;

        // Get and validate old monster component first
        Monster oldMonsterComponent = currentMonster.GetComponent<Monster>();
        if (oldMonsterComponent != null)
        {
            // Unregister before any other operations
            UnregisterMonster(oldMonsterComponent);
        }

        // Get movement component data
        MonsterMovement oldMovement = currentMonster.GetComponent<MonsterMovement>();
        if (oldMovement == null)
        {
            Debug.LogError("No MonsterMovement component found on current monster");
            return;
        }

        // Store necessary data
        int endPathNumber = oldMovement.endPathNumber;
        CastleHealth castleHealth = oldMovement.castleHealth;

        // Create new monster
        string newMonsterName = type == MonsterType.Ethernal 
            ? $"InvisibleMonster{tier}" 
            : $"{type}Monster{tier}";

        GameObject prefab = GetMonsterPrefab(newMonsterName);
        if (prefab == null)
        {
            Debug.LogError($"Failed to get prefab for {newMonsterName}");
            return;
        }

        GameObject newMonster = Instantiate(prefab);
        if (newMonster != null)
        {
            // Set basic properties
            newMonster.name = newMonsterName;
            newMonster.transform.position = position;
            newMonster.transform.rotation = rotation;
            newMonster.tag = "Monster";

            // Remove any existing Monster component from the prefab
            Monster existingMonster = newMonster.GetComponent<Monster>();
            if (existingMonster != null)
            {
                Destroy(existingMonster);
            }

            // Add the appropriate Monster component
            Monster newMonsterComponent = null;
            switch (type)
            {
                case MonsterType.Health:
                    newMonsterComponent = newMonster.AddComponent<HealthMonster>();
                    break;
                case MonsterType.Strength:
                    newMonsterComponent = newMonster.AddComponent<StrengthMonster>();
                    break;
                case MonsterType.Speed:
                    newMonsterComponent = newMonster.AddComponent<SpeedMonster>();
                    break;
                case MonsterType.Ethernal:
                    newMonsterComponent = newMonster.AddComponent<EthernalMonster>();
                    break;
            }

            if (newMonsterComponent == null)
            {
                Debug.LogError($"Failed to add monster component for type {type}");
                Destroy(newMonster);
                return;
            }

            // Set up movement
            MonsterMovement newMovement = newMonster.GetComponent<MonsterMovement>();
            if (newMovement == null)
            {
                newMovement = newMonster.AddComponent<MonsterMovement>();
            }
            
            // Initialize movement with stored data
            newMovement.Initialize(0, newMonster, endPathNumber);
            newMovement.castleHealth = castleHealth;

            // Register new monster before destroying old one
            RegisterMonster(newMonsterComponent);

            // Unregister and destroy old monster
            if (oldMonsterComponent != null)
            {
                UnregisterMonster(oldMonsterComponent);
            }
            
            Destroy(currentMonster);
        }
        else
        {
            Debug.LogError($"Failed to create new monster of type {type} and tier {tier}");
        }
    }

    private void CleanupDestroyedMonsters()
    {
        monsters.RemoveAll(monster => monster == null || monster.gameObject == null);
    }
}
