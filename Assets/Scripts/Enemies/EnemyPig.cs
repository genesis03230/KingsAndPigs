using UnityEngine;

public class EnemyPig : Enemy
{
    private static readonly int XVelocity = Animator.StringToHash("xVelocity");

    protected override void Update()
    {
        base.Update();
        
        animator.SetFloat(XVelocity, rigidBody2D.linearVelocityX);
        HandleMovement();
        HandleCollision();
        
        if (isGrounded)
            HandleTurnAround();
    }

    private void HandleTurnAround()
    {
        if (!isGroundInFrontDetected || isWallDetected)
        {
            if (!isGrounded) return;
            
            Flip();
            idleTimer = idleDuration;
            rigidBody2D.linearVelocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        if (idleTimer > 0) return;

        rigidBody2D.linearVelocity = new Vector2(moveSpeed * facingDirection, rigidBody2D.linearVelocityY);
    }
}
