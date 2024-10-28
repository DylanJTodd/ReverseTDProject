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

    public override void Upgrade(int tier)
    {
        switch(tier)
        {
            case 1:
                health = (int)(health * 1.2f);
                maxHealth = (int)(maxHealth * 1.2f);
                damage = (int)(damage * 1.2f);
                break;
            case 2:
                health = (int)(health * 1.5f);
                maxHealth = (int)(maxHealth * 1.5f);
                damage = (int)(damage * 1.5f);
                break;
            case 3:
                health = (int)(health * 2f);
                maxHealth = (int)(maxHealth * 2f);
                damage = (int)(damage * 2f);
                break;
        }
    }
}

