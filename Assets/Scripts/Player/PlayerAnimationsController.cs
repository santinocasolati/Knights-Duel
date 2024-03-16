using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    [SerializeField] private Collider swordCollider;
    [SerializeField] private SwordController swordController;

    private Animator animator;
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
        animator.SetTrigger("Jump");
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
        animator.SetTrigger("Attack");
    }
}
