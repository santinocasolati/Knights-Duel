using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : NetworkBehaviour
{
    // Damage parameter in the sword for possible weapon variations
    public int damage = 1;

    // This parameter avoids hitting another player multiple times in an attack
    private bool playerHitted = false;

    public override void OnStartClient()
    {
        // Destroy if it's not from the owner to avoid miscalculation of triggers
        if (!base.IsOwner) Destroy(this);
    }

    public void ResetAttack()
    {
        playerHitted = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerHitted) return;

        PlayerHealthController phc = other.gameObject.GetComponent<PlayerHealthController>();

        if (phc == null) return;

        phc.Damage(damage, base.OwnerId);

        playerHitted = true;
    }
}
