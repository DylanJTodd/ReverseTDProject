using UnityEngine;
using System;

public class EthernalMonster : Monster
{    
    public override void Start()
    {
        base.Start();
    }
    public override void Attack()
    {
        // Health monster's attack implementation
        // This could be a basic attack or a special health-drain attack
    }

    public override void Upgrade(int tier)
    {
        // Replace current monster with higher tier ethernal monster prefab
        MonsterManager.instance.ReplaceMonster(gameObject, MonsterType.Ethernal, tier);
    }

    public override MonsterType GetMonsterType()
    {
        return MonsterType.Ethernal;
    }
}
