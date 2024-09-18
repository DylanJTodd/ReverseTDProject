using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[System.Serializable]
public class Monster
{
    public string name;
    public GameObject obj;
    public float heightAdjust; //Height value from origin to feet
    public float movementSpeed;

    public Monster(string monsterName, GameObject gameObject, float heightAdjustValue = 0, float speed = 1)
    {
        name = monsterName;
        obj = gameObject;
        heightAdjust = heightAdjustValue;
        movementSpeed = speed;
    }
}
