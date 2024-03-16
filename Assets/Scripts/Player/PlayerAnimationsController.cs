using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsController : NetworkBehaviour
{
    [Header("Sword Settings")]
    [SerializeField] private Collider swordCollider;
    [SerializeField] private SwordController swordController;
    [SerializeField] private AnimationClip attackAnimation;

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private bool isAttacking = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
    }

    // Using NetworkAnimator to set the triggers because, as they are setted and unsetted in a frame, the networkAnimator does not reach to update the triggers
    // Primitive values are setted in the normal animator because they are updated correctly
    // The owners are the only ones updating the triggers. The ServerRpc and ObserverRpc are for hearing the SFX in both clients

    public void SetMoveDirection(Vector2 direction)
    {
        // Parameters passed to a blend tree to blend the movement animations
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveZ", direction.y);
    }

    public void PerformJump()
    {
        networkAnimator.SetTrigger("Jump");
    }

    public void ResetAttackState()
    {
        isAttacking = false;
        swordCollider.enabled = false;
    }

    public void PerformAttack()
    {
        // If the trigger is setted when its on the attack state the animation performs twice
        if (isAttacking) return;
        isAttacking = true;
        swordCollider.enabled = true;
        swordController.ResetAttack();
        networkAnimator.SetTrigger("Attack");

        Invoke(nameof(ResetAttackState), attackAnimation.length);

        AttackServer();
    }

    [ServerRpc]
    private void AttackServer()
    {
        AttackObserver();
    }

    [ObserversRpc]
    private void AttackObserver()
    {
        AudioManager.instance.PlaySound("sword");
    }

    public void PlayerHitted()
    {
        networkAnimator.SetTrigger("Hit");
        PlayerHittedServer();
    }

    [ServerRpc]
    private void PlayerHittedServer()
    {
        PlayerHittedObserver();
    }

    [ObserversRpc]
    private void PlayerHittedObserver()
    {
        AudioManager.instance.PlaySound("hit");
    }

    public void PlayerKilled()
    {
        networkAnimator.SetTrigger("Death");
        PlayerKilledServer();
    }

    [ServerRpc]
    private void PlayerKilledServer()
    {
        PlayerKilledObserver();
    }

    [ObserversRpc]
    private void PlayerKilledObserver()
    {
        AudioManager.instance.PlaySound("death");
    }
}
