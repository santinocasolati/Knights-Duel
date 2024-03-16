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

    public override void OnStartNetwork()
    {
        ResetHp();
    }

    public void ResetHp()
    {
        currentHP = maxHP;
    }

    private void UpdateHPBar(int oldHP, int newHP, bool asServer)
    {
        OnPlayerHealthModified?.Invoke(currentHP);
    }

    public void Damage(int damage, int attackerId)
    {
        // Prevent the player of damaging himself or be damaged while is not started
        if (base.IsOwner || !base.OnStartClientCalled) return;
        DamageServer(damage, attackerId);
    }

    // The player that damages the other sends a signal to the server to update the hp
    // The server sends a signal to the players to perform operations
    [ServerRpc(RequireOwnership = false)]
    private void DamageServer(int damage, int attackerId)
    {
        currentHP -= damage;
        DamageObserver(currentHP, attackerId);
    }

    [ObserversRpc]
    private void DamageObserver(int hp, int attackerId)
    {
        if (!base.IsOwner) return;

        if (hp == 0)
        {
            OnPlayerKilled?.Invoke();
            PlayerManager.PlayerDied(base.OwnerId, attackerId);
        }
        else
        {
            OnPlayerDamaged?.Invoke();
        }
    }
}
