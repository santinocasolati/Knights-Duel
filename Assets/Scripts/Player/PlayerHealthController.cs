using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private int maxHP = 10;

    public UnityEvent OnPlayerDamaged;
    public UnityEvent OnPlayerKilled;
    public UnityEvent<int> OnPlayerHealthModified;

    [SyncVar(OnChange = nameof(UpdateHPBar))]private int currentHP;

    private void Start()
    {
        OnPlayerKilled.AddListener(PlayerDeathHandler);
    }

    public override void OnStartClient()
    {
        currentHP = maxHP;
    }

    private void UpdateHPBar(int oldHP, int newHP, bool asServer)
    {
        OnPlayerHealthModified?.Invoke(currentHP);
    }

    private void PlayerDeathHandler()
    {
        // Prevent damage to dead player. Gameobject is not destroyed to perform a death animation
        Destroy(this);
    }

    public void Damage(int damage)
    {
        // Prevent the player of damaging himself or be damaged while is not started
        if (base.IsOwner || !base.OnStartClientCalled) return;
        DamageServer(damage);
    }

    // The player that damages the other sends a signal to the server to update the hp
    // The server sends a signal to the players to perform operations
    [ServerRpc(RequireOwnership = false)]
    private void DamageServer(int damage, NetworkConnection conn = null)
    {
        currentHP -= damage;
        DamageObserver();
    }

    // Only the owner calls the events to modify its animations
    [ObserversRpc]
    private void DamageObserver()
    {
        if (!base.IsOwner) return;

        if (currentHP <= 0)
        {
            OnPlayerKilled?.Invoke();
        }
        else
        {
            OnPlayerDamaged?.Invoke();
        }
    }
}
