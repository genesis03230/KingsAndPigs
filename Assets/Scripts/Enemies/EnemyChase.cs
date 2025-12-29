using UnityEngine;

public class EnemyChase : Enemy
{
    private static readonly int XVelocity = Animator.StringToHash("xVelocity");

    [Header("EnemyChase Settings")] 
    [SerializeField] private float aggroDuration;
    [SerializeField] private float detectionRange;
    private float aggroTimer;
    private bool playerDetected; //MODIFICADO
    private bool canFlip = true;

    protected override void Update()
    {
        base.Update();
        aggroTimer -= Time.deltaTime;
        
        if (isDead) return;

        if (playerDetected)
        {
            canMove = true;
            aggroTimer = aggroDuration;
        }
        
        if (aggroTimer <= 0)
            canMove = false;
        
        HandleCollision();
        HandleMovement();
        
        float xVelocity = Mathf.Abs(rigidBody2D.linearVelocity.x);
        if (!isGrounded)
            xVelocity = 0f;

        animator.SetFloat(XVelocity, xVelocity);
        
        if (isGrounded)
        {
            HandleTurnAround();
        }
    }

    private void HandleTurnAround()
    {
        if (!isGroundInFrontDetected || isWallDetected)
        {
            if (!isGrounded) return;
            
            Flip();
            canMove = false;
            idleTimer = idleDuration;
            rigidBody2D.linearVelocity = Vector2.zero;
        }
    }

    private void HandleMovement()
    {
        if (!canMove) return;
        if (idleTimer > 0) return;
        if (!isGrounded) return;
        
        if (player != null)
            HandleFlip(player.position.x); //AGREGADO
        
        rigidBody2D.linearVelocity = new Vector2(moveSpeed * facingDirection, rigidBody2D.linearVelocityY);
    }

    protected override void HandleFlip(float xValue) //AGREGADO
    {
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
        {
            if (canFlip)
            {
                canFlip = false;
                Invoke(nameof(Flip), 0.3f);
            }
        }
    }

    protected override void Flip() //AGREGADO
    {
        base.Flip();
        canFlip = true;
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        playerDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, detectionRange, whatIsPlayer);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (detectionRange * facingDirection), transform.position.y));
    }
    
    protected virtual void OnDisable() //AGREGADO
    {
        CancelInvoke(nameof(UpdatePlayerRef));
    }
}
