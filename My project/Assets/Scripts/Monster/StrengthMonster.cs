using UnityEngine;

public class StrengthMonster : Monster
{
    private void Start()
    {
        // Initialize base stats
        health = 100;
        maxHealth = 100;
        damage = 15;
        movementSpeed = 0.9f;
        cost = 20;
    }

    public override void Attack()
    {
        // Strength monster's attack implementation
        // This could be a powerful single-target attack
    }

    public override void Upgrade(int tier)
    {
        switch(tier)
        {
            case 1:
                damage = (int)(damage * 1.3f);
                health = (int)(health * 1.1f);
                maxHealth = (int)(maxHealth * 1.1f);
                break;
            case 2:
                damage = (int)(damage * 1.6f);
                health = (int)(health * 1.3f);
                maxHealth = (int)(maxHealth * 1.3f);
                break;
            case 3:
                damage = (int)(damage * 2.2f);
                health = (int)(health * 1.5f);
                maxHealth = (int)(maxHealth * 1.5f);
                break;
        }
    }
}

