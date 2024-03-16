using FishNet.Component.Animating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    [Header("Sword Settings")]
    [SerializeField] private Collider swordCollider;
    [SerializeField] private SwordController swordController;

    private Animator animator;
    private NetworkAnimator networkAnimator;
    private bool isAttacking = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        // Parameters passed to a blend tree to blend the movement animations
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveZ", direction.y);
    }

    public void PerformJump()
    {
        // Using NetworkAnimator to set the triggers because, as they are setted and unsetted in a frame, the networkAnimator does not reach to update the triggers
        // Primitive values are setted in the normal animator because they are updated correctly
        networkAnimator.SetTrigger("Jump");
    }

    public void SetAttackState(bool state)
    {
        // Called from animation state events
        isAttacking = state;
        swordCollider.enabled = state;
    }

    public void PerformAttack()
    {
        // If the trigger is setted when its on the attack state the animation performs twice
        if (isAttacking) return;
        swordController.ResetAttack();
        networkAnimator.SetTrigger("Attack");
        AudioManager.instance.PlaySound("sword");
    }

    public void PlayerHitted()
    {
        networkAnimator.SetTrigger("Hit");
        AudioManager.instance.PlaySound("hit");
    }

    public void PlayerKilled()
    {
        networkAnimator.SetTrigger("Death");
        AudioManager.instance.PlaySound("death");
    }
}
