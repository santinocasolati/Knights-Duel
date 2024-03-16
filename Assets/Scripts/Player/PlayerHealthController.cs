using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthController : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;

    public UnityEvent OnPlayerDamaged;
    public UnityEvent OnPlayerKilled;

    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void Damage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            OnPlayerKilled?.Invoke();
        } else
        {
            OnPlayerDamaged?.Invoke();
        }
    }
}
