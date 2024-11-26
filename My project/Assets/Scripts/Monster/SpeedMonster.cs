using UnityEngine;
using System;

public class SpeedMonster : Monster
{
    public override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        // Speed monster's attack implementation
        // This could be a quick multi-hit attack
    }

    public override void Upgrade(int tier)
    {
        // Replace current monster with higher tier speed monster prefab
        MonsterManager.instance.ReplaceMonster(gameObject, MonsterType.Speed, tier);
    }

    public override MonsterType GetMonsterType()
    {
        return MonsterType.Speed;
    }
}

