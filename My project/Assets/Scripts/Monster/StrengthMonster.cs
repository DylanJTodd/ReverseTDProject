using UnityEngine;

public class StrengthMonster : Monster
{
    public override void Start()
    {
        // Initialize base stats
        health = 100;
        maxHealth = 100;
        damage = 15;
        movementSpeed = 0.9f;
        cost = 20;
        base.Start();
    }

    public override void Attack()
    {
        // Strength monster's attack implementation
        // This could be a powerful single-target attack
    }

    public override void Upgrade(int tier)
    {
        damage += damage * tier;
    }
}

