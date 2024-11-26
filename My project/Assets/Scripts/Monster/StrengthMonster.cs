using UnityEngine;
using System;

public class StrengthMonster : Monster
{
    public override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        // Strength monster's attack implementation
        // This could be a powerful single-target attack
    }

    public override void Upgrade(int tier)
    {
        // Replace current monster with higher tier strength monster prefab
        MonsterManager.instance.ReplaceMonster(gameObject, MonsterType.Strength, tier);
    }

    public override MonsterType GetMonsterType()
    {
        return MonsterType.Strength;
    }
}

