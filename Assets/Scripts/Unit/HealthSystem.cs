using UnityEngine;
using System;

/// <summary>
/// This component is attached to agents and exists to track the health of the agent during the scene.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnDeath;
    public event EventHandler OnDamaged;
    private Unit unit;
    [SerializeField] private int health = 100;
    private int healthMax = 100;

    private void Awake()
    {
        healthMax = health;
        unit = GetComponent<Unit>();
    }

    public void Damage(int damageAmount)
    {
        health -= damageAmount;

        string logText = unit + " was hurt for " + damageAmount + ".\n";
        LogUI.Instance.UpdateLog(logText);

        if (health <= 0)
        {
            health = 0;
        }

        OnDamaged?.Invoke(this, EventArgs.Empty);

        if (health == 0)
        {
            logText = unit + " has been destroyed.";
            LogUI.Instance.UpdateLog(logText);
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }
}
