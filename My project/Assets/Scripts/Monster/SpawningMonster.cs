using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningMonster : MonoBehaviour
{
    public MonsterManager monsterManager;
    public GameObject lineStart;
    public GameObject lineEnd;
    public Transform monsterParent;
    public CastleHealth castleHealth;
    public int endPathNumber;

    public void SpawnMonster(string name)
    {
        Debug.Log($"Attempting to spawn monster: {name}");
        
        if (monsterManager == null)
        {
            Debug.LogError("MonsterManager reference is missing!");
            return;
        }

        if (lineStart == null || lineEnd == null)
        {
            Debug.LogError("Line start or end references are missing!");
            return;
        }

        GameObject monsterObject = monsterManager.CreateNewMonster(name);
        if (monsterObject == null)
        {
            Debug.LogError($"Failed to create monster object for: {name}");
            return;
        }

        Monster monster = monsterObject.GetComponent<Monster>();
        if (monster == null)
        {
            Debug.LogError($"Failed to get Monster component for: {name}");
            Destroy(monsterObject);
            return;
        }

        Debug.Log($"Monster created successfully: {name}");

        float interpolate = Random.Range(0f, 1f);
        Vector3 spawnPosition = Vector3.Lerp(lineStart.transform.position, lineEnd.transform.position, interpolate);
        Vector3 spawnAdjustment = new Vector3(0, monster.heightAdjust, 0);
        spawnPosition += spawnAdjustment;

        monsterObject.transform.position = spawnPosition;
        
        if (monsterParent != null)
        {
            monsterObject.transform.SetParent(monsterParent);
        }
        else
        {
            Debug.LogWarning("Monster parent transform is not set!");
        }

        monsterObject.tag = "Monster";

        MonsterMovement movementScript = monsterObject.AddComponent<MonsterMovement>();
        if (movementScript != null)
        {
            movementScript.castleHealth = castleHealth;
            movementScript.Initialize(interpolate, monsterObject, endPathNumber);
            Debug.Log($"Monster movement initialized: {name}");
        }
        else
        {
            Debug.LogError("Failed to add MonsterMovement component!");
        }
    }

}
