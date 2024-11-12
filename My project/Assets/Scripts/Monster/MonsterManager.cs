using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterManager : MonoBehaviour
{
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

    public List<Monster> GetMonsters()
    {
        return monsters;
    }
}
