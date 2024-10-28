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
        GameObject monsterObject = monsterManager.GetMonsterByName(name);
        Monster monster = monsterObject.GetComponent<Monster>();

        float interpolate = Random.Range(0f, 1f);
        Vector3 spawnPosition = Vector3.Lerp(lineStart.transform.position, lineEnd.transform.position, interpolate);

        Vector3 spawnAdjustment = new Vector3(0, monster.heightAdjust, 0);
        spawnPosition += spawnAdjustment;

        GameObject spawnedMonster = Instantiate(monsterObject, spawnPosition, Quaternion.identity);
        spawnedMonster.transform.SetParent(monsterParent);
        spawnedMonster.tag = "Monster";

        MonsterMovement movementScript = spawnedMonster.AddComponent<MonsterMovement>();
        movementScript.castleHealth = castleHealth;
        movementScript.Initialize(interpolate, spawnedMonster, endPathNumber);
    }

}
