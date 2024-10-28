using UnityEngine;

public class SpeedMonster : Monster
{
    private void Start()
    {
        // Initialize base stats
        health = 80;
        maxHealth = 80;
        damage = 12;
        movementSpeed = 1.3f;
        cost = 18;
    }

    public override void Attack()
    {
        // Speed monster's attack implementation
        // This could be a quick multi-hit attack
    }

    public override void Upgrade(int tier)
    {
        switch(tier)
        {
            case 1:
                movementSpeed *= 1.2f;
                damage = (int)(damage * 1.1f);
                break;
            case 2:
                movementSpeed *= 1.5f;
                damage = (int)(damage * 1.3f);
                break;
            case 3:
                movementSpeed *= 2f;
                damage = (int)(damage * 1.5f);
                break;
        }
    }
}

