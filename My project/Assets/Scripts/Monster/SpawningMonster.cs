using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningMonster : MonoBehaviour
{
    public MonsterManager monsterManager;
    public GameObject lineStart;
    public GameObject lineEnd;
    public Transform monsterParent;

    public void SpawnMonster(string name)
    {
        Monster monster = monsterManager.GetMonsterByName(name);

        float interpolate = Random.Range(0f, 1f);
        Vector3 spawnPosition = Vector3.Lerp(lineStart.transform.position, lineEnd.transform.position, interpolate);
        Vector3 spawnAdjustment = new Vector3(0, monster.heightAdjust, 0);
        spawnPosition += spawnAdjustment;

        GameObject spawnedMonster = Instantiate(monster.obj, spawnPosition, Quaternion.identity);
        spawnedMonster.transform.SetParent(monsterParent);

        MonsterMovement movementScript = spawnedMonster.AddComponent<MonsterMovement>();
        movementScript.Initialize(interpolate, monster);
    }
}
    