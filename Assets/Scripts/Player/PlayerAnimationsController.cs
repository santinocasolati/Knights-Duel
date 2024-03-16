using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationsController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoveDirection(Vector2 direction)
    {
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveZ", direction.y);
    }

    public void PerformJump()
    {
        animator.SetTrigger("Jump");
    }

    public void PerformAttack()
    {
        // If the trigger is setted when its on the attack state the animation performs twice
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Attack")) return;
        animator.SetTrigger("Attack");
    }
}