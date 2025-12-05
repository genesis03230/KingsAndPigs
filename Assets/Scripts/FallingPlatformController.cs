using UnityEngine;

public class FallingPlatformController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private BoxCollider2D[] _boxCollider2D;
    
    [SerializeField] private float speed = 0.75f;
    [SerializeField] private float travelDistance;
    private Vector3[] _wayPoints;
    private int _wayPointIndex;
    public bool canMove;
    
    [Header("Platform Fall Details")]
    [SerializeField] private float fallDelay = 0.5f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _boxCollider2D = GetComponents<BoxCollider2D>();
    }
    
    private void Start()
    {
        SetupWaypoints();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void SetupWaypoints()
    {
        _wayPoints = new Vector3[2];
        float yOffset = travelDistance / 2;
        _wayPoints[0] = transform.position + new Vector3(0, yOffset, 0);
        _wayPoints[1] = transform.position + new Vector3(0, -yOffset, 0);
    }

    private void HandleMovement()
    {
        if (!canMove) return;
        
        transform.position = Vector2.MoveTowards(transform.position, _wayPoints[_wayPointIndex], speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, _wayPoints[_wayPointIndex]) < 0.1f)
        {
            _wayPointIndex++;
            if (_wayPointIndex >= _wayPoints.Length)
                _wayPointIndex = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController  player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            Invoke(nameof(SwitchOffPlatform), fallDelay);
        }
            
    }

    private void SwitchOffPlatform()
    {
        _animator.SetTrigger("deactivate");
        canMove = false;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = 3.5f;
        _rigidbody2D.linearDamping = 0.5f;

        foreach (BoxCollider2D collider in _boxCollider2D)
        {
            collider.enabled = false;
        }
    }
    
}
