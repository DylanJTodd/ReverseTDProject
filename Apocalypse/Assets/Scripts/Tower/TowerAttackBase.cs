using UnityEngine;

public abstract class TowerAttackBase : MonoBehaviour
{
    public float towerRadius;
    public float baseDamage;
    public float attackCooldown;
    protected int tier = 1;

    public virtual float GetDPS()
    {
        return baseDamage / attackCooldown;
    }

    public virtual int GetTier()
    {
        return tier;
    }

    public virtual void SetTier(int newTier)
    {
        tier = newTier;
        UpdateStats();
    }

    public virtual int GetCost()
    {
        return 750 * tier;
    }

    protected virtual void UpdateStats()
    {

    }
}