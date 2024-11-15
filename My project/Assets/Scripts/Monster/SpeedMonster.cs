using UnityEngine;

public class SpeedMonster : Monster
{
    public override void Start()
    {
        // Initialize base stats
        health = 80;
        maxHealth = 80;
        damage = 12;
        movementSpeed = 1.3f;
        cost = 18;
        base.Start();
    }

    public override void Attack()
    {
        // Speed monster's attack implementation
        // This could be a quick multi-hit attack
    }

    public override void Upgrade(int tier)
    {
        movementSpeed += movementSpeed * tier;
        maxSpeed += maxSpeed * tier;
    }

}

