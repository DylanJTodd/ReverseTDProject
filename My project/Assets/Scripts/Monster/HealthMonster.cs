using UnityEngine;

public class HealthMonster : Monster
{
    private void Start()
    {
        // Initialize base stats
        health = 150;
        maxHealth = 150;
        damage = 8;
        movementSpeed = 0.8f;
        cost = 15;
    }

    public override void Attack()
    {
        // Health monster's attack implementation
        // This could be a basic attack or a special health-drain attack
    }
}

