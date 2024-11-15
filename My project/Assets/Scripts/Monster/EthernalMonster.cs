using UnityEngine;

public class EthernalMonster : Monster
{    
    public override void Start()
    {
        health = 100;
        maxHealth = 100;
        damage = 10;
        movementSpeed = 0.5f;
        cost = 20;
        base.Start();
    }
    public override void Attack()
    {
        // Health monster's attack implementation
        // This could be a basic attack or a special health-drain attack
    }

    public override void Upgrade(int tier)
    {
        health += health * tier;
        maxHealth += maxHealth * tier;
    }
}
