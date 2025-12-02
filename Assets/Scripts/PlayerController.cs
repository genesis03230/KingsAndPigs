using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Components")] 
    [SerializeField] private Transform myTransform; //Referencia al Transform del Player
    private Rigidbody2D _rigidbody2D; //Referencia al Rigidbody del Player
    private GatherInput _gatherInput; //Referencia al GatherInput
    private Animator _animator; //Referencia al Animator del Player
    
    //ANIMATOR IDS
    private readonly int _idIsGrounded = Animator.StringToHash("isGrounded");
    private readonly int _idSpeed = Animator.StringToHash("speed");
    private readonly int _idIsWallDetected = Animator.StringToHash("isWallDetected");
    private readonly int _idKnockback = Animator.StringToHash("isKnockback");
    private readonly int _idIdle = Animator.StringToHash("Idle");
    private readonly int _idDoorIn = Animator.StringToHash("doorIn");

    [Header("Move Settings")] 
    [SerializeField] private float speed; //Velocidad de movimiento
    [SerializeField] private bool canMove;
    [SerializeField] private float moveDelay;
    private int _direction = 1; //Direccion de movimiento

    [Header("Jump Settings")] 
    [SerializeField] private float jumpForce; //Fuerza de salto
    [SerializeField] private int extraJumps; //Saltos extras
    [SerializeField] private int counterExtraJumps; //Conteo de Saltos extras
    [SerializeField] private bool canDoubleJump; //Detectar si puedo hacer doble salto

    [Header("Ground Settings")] 
    [SerializeField] private Transform lFoot;
    [SerializeField] private Transform rFoot;
    private RaycastHit2D _lFootRay;
    private RaycastHit2D _rFootRay;
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
    //[SerializeField] private bool canBeKnocked;
    [SerializeField] private Vector2 defaultKnockedPower;
    [SerializeField] private float knockedDuration;
    private Vector2 _knockedPower;
    public Vector2 KnockedPower { get => _knockedPower; set => _knockedPower = value; }

    [Header("Death VFX")] 
    [SerializeField] private GameObject deathVFX;

    private void Awake()
    {
        _knockedPower = defaultKnockedPower;
        _gatherInput = GetComponent<GatherInput>();
        myTransform = GetComponent<Transform>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        CheckPlayerRespawnState();
    }
    
    private void Start() => counterExtraJumps = extraJumps;
   
    private void CheckPlayerRespawnState()
    {
        if (GameManager.Instance.hasCheckPointActive)
        {
            canMove = true;
            StartInCheckpoint();
        }
        else
        {
            canMove = false;
            StartCoroutine(CanMoveRoutine());
        }
    }

    private void StartInCheckpoint() => _animator.Play(_idIdle);

    private void Update()
    {
        SetAnimatorValues();
    }

    private void SetAnimatorValues()
    {
        _animator.SetFloat(_idSpeed, Mathf.Abs(_rigidbody2D.linearVelocityX));
        _animator.SetBool(_idIsGrounded, isGrounded);
        _animator.SetBool(_idIsWallDetected, isWallDetected);
    }
    
    private void FixedUpdate()
    {
        if (!canMove)
        {
            HandleGround(); 
            HandleWall(); 
            SetAnimatorValues(); 
            return; 
        }
        if (isKnocked) return; //Si el personaje esta noqueado, no ejecuta ningun metodo siguiente
        CheckCollision();
        Move();
        Jump();
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
        else isGrounded = false;
    }

    private void HandleWall()
    {
        isWallDetected = Physics2D.Raycast(myTransform.position, Vector2.right * _direction, checkWallDistance, groundLayer);
    }

    private void HandleWallSlide()
    {
        canWallSlide = isWallDetected;
        if (!canWallSlide) return;
        canDoubleJump = false;
        slideSpeed = _gatherInput.Value.y < 0 ? 1 : 0.5f;
        _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocityX, _rigidbody2D.linearVelocityY * slideSpeed);
    }

    private void Move()
    {
        if (!canMove) return;
        if (isWallDetected && !isGrounded) return; //Esto sale del metodo y no me permite hacer flip estando en una pared
        if (isWallJumping) return;
        Flip();
        _rigidbody2D.linearVelocity = new Vector2(speed * _gatherInput.Value.x, _rigidbody2D.linearVelocityY);
    }

    private IEnumerator CanMoveRoutine()
    {
        yield return new WaitForSeconds(moveDelay);
        canMove = true;
    }

    private void Flip()
    {
        if (_gatherInput.Value.x * _direction < 0) HandleDirection();
    }

    private void HandleDirection()
    {
        myTransform.localScale = new Vector3(-myTransform.localScale.x, 1, 1);
        _direction *= -1;
    }

    private void Jump()
    {
        if (_gatherInput.IsJumping)
        {
            if (isGrounded)
            {
                _rigidbody2D.linearVelocity = new Vector2(speed * _gatherInput.Value.x, jumpForce);
                canDoubleJump = true;
            }
            else if (isWallDetected)
            {
                WallJump();
            }
            else if (counterExtraJumps > 0 && canDoubleJump) DoubleJump();
        }
        _gatherInput.IsJumping = false;
    }

    private void WallJump()
    {
        _rigidbody2D.linearVelocity = new Vector2(wallJumpForce.x * -_direction, wallJumpForce.y);
        HandleDirection();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true; //Si he saltado desde la pared
        yield return new WaitForSeconds(wallJumpDuration); //Esperar un tiempo predeterminado
        isWallJumping = false; //Y recuperar el control del Player
        Flip();
    }

    private void DoubleJump()
    {
        _rigidbody2D.linearVelocity = new Vector2(speed * _gatherInput.Value.x, jumpForce);
        counterExtraJumps--;
    }

    public void Knockback()
    {
        StartCoroutine(KnockbackRoutine());
        _rigidbody2D.linearVelocity = new Vector2(_knockedPower.x * -_direction, _knockedPower.y);
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;
        _animator.SetBool(_idKnockback, isKnocked);
        yield return new WaitForSeconds(knockedDuration);
        isKnocked = false;
        _animator.SetBool(_idKnockback, isKnocked);
        _knockedPower = new Vector2(defaultKnockedPower.x, defaultKnockedPower.y);
    }

    public void Die()
    {
        var deathVFXPrefab = Instantiate(deathVFX, myTransform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    
    public void Push(Vector2 direction, float duration = 0)
    {
        StartCoroutine(PushCouroutine(direction, duration));
    }

    public IEnumerator PushCouroutine(Vector2 direction, float duration)
    {
        canMove = false;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.AddForce(direction, ForceMode2D.Impulse);
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    public void DoorIn()
    {
        _rigidbody2D.linearVelocity = Vector2.zero;
        _animator.Play(_idIdle);
        _animator.SetBool(_idDoorIn, true);
        canMove = false;
        StartCoroutine(DoorInRoutine());
    }

    private IEnumerator DoorInRoutine()
    {
        yield return new WaitForSeconds(moveDelay);
        SceneManager.LoadScene(0);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(myTransform.position,
            new Vector2(myTransform.position.x + checkWallDistance * _direction, myTransform.position.y));
    }
}
