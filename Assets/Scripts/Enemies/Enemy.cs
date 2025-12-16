using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Animator animator;
    protected Rigidbody2D rigidBody2D;
    
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float idleDuration;
    protected float idleTimer;
    
    [Header("Basic Collision")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = 0.7f;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected Transform groundCheck;
    protected bool isGrounded;
    protected bool isWallDetected;
    protected bool isGroundInFrontDetected;
    
    protected int facingDirection = -1;
    protected bool facingRight = false;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        idleTimer -= Time.deltaTime;
    }

    protected virtual void HandleFlip(float xValue)
    {
        if (xValue < 0 && facingRight || xValue > 0 && !facingRight)
            Flip();
    }
    
    protected virtual void Flip()
    {
        facingDirection = facingDirection * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundInFrontDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDirection), transform.position.y));
    }
}
