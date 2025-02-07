using System;
using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Lumin;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform m_transform; //Referencia al Transform del Player
    private Rigidbody2D m_rigidbody2D; //Referencia al Rigidbody del Player
    private GatherInput m_gatherInput; //Referencia al GatherInput
    private Animator m_animator; //Referencia al Animator del Player

    //ANIMATOR IDS
    private int idSpeed;
    private int idIsGrounded;
    private int idIsWallDetected;
    private int idKnockback;

    [Header("Move Settings")]
    [SerializeField] private float speed; //Velocidad de movimiento
    private int direction = 1; //Direccion de movimiento

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce; //Fuerza de salto
    [SerializeField] private int extraJumps; //Saltos extras
    [SerializeField] private int counterExtraJumps; //Conteo de Saltos extras
    [SerializeField] private bool canDoubleJump; //Detectar si puedo hacer doble salto

    [Header("Ground Settings")]
    [SerializeField] private Transform lFoot;
    [SerializeField] private Transform rFoot;
    RaycastHit2D lFootRay;
    RaycastHit2D rFootRay;
    [SerializeField] private bool isGrounded; //Detectar si esta en el suelo
    [SerializeField] private float rayLength; //Distancia del rayo
    [SerializeField] private LayerMask groundLayer; //Layer del suelo

    [Header("Wall Settings")]
    [SerializeField] private float checkWallDistance; //Chequear la distancia de pared
    [SerializeField] private bool isWallDetected; //Detectar pared
    [SerializeField] private bool canWallSlide; //Comprobar si puedo deslizarme por pared
    [SerializeField] private float slideSpeed; //Velocidad de deslizamiento por pared
    [SerializeField] private Vector2 wallJumpForce; //Fuerza de salto desde la pared
    [SerializeField] private bool isWallJumping; //Detectar si puedo saltar desde la pared
    [SerializeField] private float wallJumpDuration; //Duracion del salto de la pared para recuperar el control del Player

    [Header("Knock Settings")]
    [SerializeField] private bool isKnocked;
    [SerializeField] private bool canBeKnocked;
    [SerializeField] private Vector2 knockedPower;
    [SerializeField] private float knockedDuration;



    private void Awake()
    {
        m_gatherInput = GetComponent<GatherInput>();
        //m_transform = GetComponent<Transform>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
    }

    void Start()
    {
        idSpeed = Animator.StringToHash("speed");
        idIsGrounded = Animator.StringToHash("isGrounded");
        idIsWallDetected = Animator.StringToHash("isWallDetected");
        idKnockback = Animator.StringToHash("knockback");
        lFoot = GameObject.Find("LFoot").GetComponent<Transform>();
        rFoot = GameObject.Find("RFoot").GetComponent<Transform>();
        counterExtraJumps = extraJumps;
    }

    private void Update()
    {
        SetAnimatorValues();
    }

    private void SetAnimatorValues()
    {
        m_animator.SetFloat(idSpeed, Mathf.Abs(m_rigidbody2D.linearVelocityX));
        m_animator.SetBool(idIsGrounded, isGrounded);
        m_animator.SetBool(idIsWallDetected, isWallDetected);
    }

    void FixedUpdate()
    {
        if (isKnocked) return; //Si el personaje esta noqueado, no ejecuta ningun metodo siguiente
        CheckColiision();
        Move();
        Jump();
    }

    private void CheckColiision()
    {
        HandleGround();
        HandleWall();
        HandleWallSlide();
    }

    private void HandleGround()
    {
        lFootRay = Physics2D.Raycast(lFoot.position, Vector2.down, rayLength, groundLayer);
        rFootRay = Physics2D.Raycast(rFoot.position, Vector2.down, rayLength, groundLayer);
        if (lFootRay || rFootRay)
        {
            isGrounded = true;
            counterExtraJumps = extraJumps;
            canDoubleJump = false;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void HandleWall()
    {
        isWallDetected = Physics2D.Raycast(m_transform.position, Vector2.right * direction, checkWallDistance, groundLayer);
    }

    private void HandleWallSlide()
    {
        canWallSlide = isWallDetected;
        if (!canWallSlide) return;
        canDoubleJump = false;
        slideSpeed = m_gatherInput.Value.y < 0 ? 1 : 0.5f;
        m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocityX, m_rigidbody2D.linearVelocityY * slideSpeed);
    }

    private void Move()
    {
        if (isWallDetected && !isGrounded) return; //Esto sale del metodo y no me permite hacer flip estando en una pared
        if (isWallJumping) return;

        Flip();
        m_rigidbody2D.linearVelocity = new Vector2(speed * m_gatherInput.Value.x, m_rigidbody2D.linearVelocityY);
    }

    private void Flip()
    {
        if(m_gatherInput.Value.x * direction < 0)
        {
            HandleDirection();
        }
    }

    private void HandleDirection()
    {
        m_transform.localScale = new Vector3(-m_transform.localScale.x, 1, 1);
        direction *= -1;
    }

    private void Jump()
    {
        if (m_gatherInput.IsJumping)
        {
            if (isGrounded)
            {
                m_rigidbody2D.linearVelocity = new Vector2(speed * m_gatherInput.Value.x, jumpForce);
                canDoubleJump = true;
            }
            else if (isWallDetected) WallJump();
            else if (counterExtraJumps > 0 && canDoubleJump) DoubleJump();
        }
        m_gatherInput.IsJumping = false;
    }

    private void WallJump()
    {
        m_rigidbody2D.linearVelocity = new Vector2(wallJumpForce.x * -direction, wallJumpForce.y);
        HandleDirection();
        StartCoroutine(WallJumpRoutine());
    }

    IEnumerator WallJumpRoutine()
    {
        isWallJumping = true; //Si he saltado desde la pared
        yield return new WaitForSeconds(wallJumpDuration); //Esperar un tiempo predeterminado
        isWallJumping = false; //Y recuperar el control del Player
    }

    private void DoubleJump()
    {
        m_rigidbody2D.linearVelocity = new Vector2(speed * m_gatherInput.Value.x, jumpForce);
        counterExtraJumps--;
    }

    public void Knockback()
    {
        StartCoroutine(KnockbackRoutine());
        m_rigidbody2D.linearVelocity = new Vector2(knockedPower.x * -direction, knockedPower.y);
        m_animator.SetTrigger(idKnockback);
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;
        canBeKnocked = false;
        yield return new WaitForSeconds(knockedDuration);
        isKnocked = false;
        canBeKnocked = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(m_transform.position, new Vector2(m_transform.position.x + (checkWallDistance * direction), m_transform.position.y));
    }


}
