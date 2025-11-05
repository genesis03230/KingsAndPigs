using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private Transform mTransform; //Referencia al Transform del Player

    [Header("Move Settings")] [SerializeField]
    private float speed; //Velocidad de movimiento

    [SerializeField] private bool canMove;
    [SerializeField] private float moveDelay;

    [Header("Jump Settings")] [SerializeField]
    private float jumpForce; //Fuerza de salto

    [SerializeField] private int extraJumps; //Saltos extras
    [SerializeField] private int counterExtraJumps; //Conteo de Saltos extras
    [SerializeField] private bool canDoubleJump; //Detectar si puedo hacer doble salto

    [Header("Ground Settings")] [SerializeField]
    private Transform lFoot;

    [SerializeField] private Transform rFoot;
    [SerializeField] private bool isGrounded; //Detectar si esta en el suelo
    [SerializeField] private float rayLength; //Distancia del rayo
    [SerializeField] private LayerMask groundLayer; //Layer del suelo

    [Header("Wall Settings")] [SerializeField]
    private float checkWallDistance; //Chequear la distancia de pared

    [SerializeField] private bool isWallDetected; //Detectar pared
    [SerializeField] private bool canWallSlide; //Comprobar si puedo deslizarme por pared
    [SerializeField] private float slideSpeed; //Velocidad de deslizamiento por pared
    [SerializeField] private Vector2 wallJumpForce; //Fuerza de salto desde la pared
    [SerializeField] private bool isWallJumping; //Detectar si puedo saltar desde la pared
    [SerializeField] private float wallJumpDuration; //Duracion del salto de la pared para recuperar el control del Player

    [Header("Knock Settings")] [SerializeField]
    private bool isKnocked;

    //[SerializeField] private bool canBeKnocked;
    [SerializeField] private Vector2 knockedPower;
    [SerializeField] private float knockedDuration;

    [Header("Death VFX")] [SerializeField] private GameObject deathVFX;

    private int _direction = 1; //Direccion de movimiento

    //ANIMATOR IDS
    private int _idIsGrounded;
    private int _idIsWallDetected;
    private int _idKnockback;
    private int _idSpeed;
    private RaycastHit2D _lFootRay;
    private Animator _mAnimator; //Referencia al Animator del Player
    private GatherInput _mGatherInput; //Referencia al GatherInput
    private Rigidbody2D _mRigidbody2D; //Referencia al Rigidbody del Player
    private RaycastHit2D _rFootRay;

    private void Awake()
    {
        _mGatherInput = GetComponent<GatherInput>();
        mTransform = GetComponent<Transform>();
        _mRigidbody2D = GetComponent<Rigidbody2D>();
        _mAnimator = GetComponent<Animator>();
        canMove = false;
        StartCoroutine(CanMoveRoutine());
    }

    private void Start()
    {
        _idSpeed = Animator.StringToHash("speed");
        _idIsGrounded = Animator.StringToHash("isGrounded");
        _idIsWallDetected = Animator.StringToHash("isWallDetected");
        _idKnockback = Animator.StringToHash("knockback");
        counterExtraJumps = extraJumps;
    }

    private void Update()
    {
        SetAnimatorValues();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        if (isKnocked) return; //Si el personaje esta noqueado, no ejecuta ningun metodo siguiente
        CheckCollision();
        Move();
        Jump();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(mTransform.position,
            new Vector2(mTransform.position.x + checkWallDistance * _direction, mTransform.position.y));
    }

    private void SetAnimatorValues()
    {
        _mAnimator.SetFloat(_idSpeed, Mathf.Abs(_mRigidbody2D.linearVelocityX));
        _mAnimator.SetBool(_idIsGrounded, isGrounded);
        _mAnimator.SetBool(_idIsWallDetected, isWallDetected);
    }

    private void CheckCollision()
    {
        HandleGround();
        HandleWall();
        HandleWallSlide();
    }

    private void HandleGround()
    {
        _lFootRay = Physics2D.Raycast(lFoot.position, Vector2.down, rayLength, groundLayer);
        _rFootRay = Physics2D.Raycast(rFoot.position, Vector2.down, rayLength, groundLayer);
        if (_lFootRay || _rFootRay)
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
        isWallDetected = Physics2D.Raycast(mTransform.position, Vector2.right * _direction, checkWallDistance, groundLayer);
    }

    private void HandleWallSlide()
    {
        canWallSlide = isWallDetected;
        if (!canWallSlide) return;
        canDoubleJump = false;
        slideSpeed = _mGatherInput.Value.y < 0 ? 1 : 0.5f;
        _mRigidbody2D.linearVelocity = new Vector2(_mRigidbody2D.linearVelocityX, _mRigidbody2D.linearVelocityY * slideSpeed);
    }

    private void Move()
    {
        if (!canMove) return;
        if (isWallDetected && !isGrounded) return; //Esto sale del metodo y no me permite hacer flip estando en una pared
        if (isWallJumping) return;

        Flip();
        _mRigidbody2D.linearVelocity = new Vector2(speed * _mGatherInput.Value.x, _mRigidbody2D.linearVelocityY);
    }

    private IEnumerator CanMoveRoutine()
    {
        yield return new WaitForSeconds(moveDelay);
        canMove = true;
    }

    private void Flip()
    {
        if (_mGatherInput.Value.x * _direction < 0)
        {
            HandleDirection();
        }
    }

    private void HandleDirection()
    {
        mTransform.localScale = new Vector3(-mTransform.localScale.x, 1, 1);
        _direction *= -1;
    }

    private void Jump()
    {
        if (_mGatherInput.IsJumping)
        {
            if (isGrounded)
            {
                _mRigidbody2D.linearVelocity = new Vector2(speed * _mGatherInput.Value.x, jumpForce);
                canDoubleJump = true;
            }
            else if (isWallDetected)
            {
                WallJump();
            }
            else if (counterExtraJumps > 0 && canDoubleJump) DoubleJump();
        }
        _mGatherInput.IsJumping = false;
    }

    private void WallJump()
    {
        _mRigidbody2D.linearVelocity = new Vector2(wallJumpForce.x * -_direction, wallJumpForce.y);
        HandleDirection();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true; //Si he saltado desde la pared
        yield return new WaitForSeconds(wallJumpDuration); //Esperar un tiempo predeterminado
        isWallJumping = false; //Y recuperar el control del Player
    }

    private void DoubleJump()
    {
        _mRigidbody2D.linearVelocity = new Vector2(speed * _mGatherInput.Value.x, jumpForce);
        counterExtraJumps--;
    }

    public void Knockback()
    {
        StartCoroutine(KnockbackRoutine());
        _mRigidbody2D.linearVelocity = new Vector2(knockedPower.x * -_direction, knockedPower.y);
        _mAnimator.SetTrigger(_idKnockback);
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;
        //canBeKnocked = false;
        yield return new WaitForSeconds(knockedDuration);
        isKnocked = false;
        //canBeKnocked = true;
    }

    public void Die()
    {
        var deathVFXPrefab = Instantiate(deathVFX, mTransform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
