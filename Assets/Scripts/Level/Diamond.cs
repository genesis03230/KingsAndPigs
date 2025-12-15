using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Diamond : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Rigidbody2D mRigidbody2D; //El agregado de m_ hace referencia a componentes propios del Gameobject
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private DiamondType diamondType;
    private int _idPickedDiamond;
    private int _idDiamondIndex;

    private void Awake()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>(); //Busca el componente en los hijos del Gameobject padre
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); //Busca el componente en los hijos del Gameobject padre
        _idPickedDiamond = Animator.StringToHash("pickedDiamond");
        _idDiamondIndex = Animator.StringToHash("diamondIndex");
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        SetRandomDiamond();
    }

    private void SetRandomDiamond()
    {
        if (!GameManager.Instance.DiamondHaveRandomLook())
        {
            UpdateDiamondType();
            return;
        }
        var randomDiamondIndex = Random.Range(0, 7);
        animator.SetFloat(_idDiamondIndex, randomDiamondIndex);
    }

    private void UpdateDiamondType()
    {
        animator.SetFloat(_idDiamondIndex, (int)diamondType);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        //spriteRenderer.enabled = false;
        mRigidbody2D.simulated = false;
        gameManager.AddDiamond();
        animator.SetTrigger(_idPickedDiamond);
    }
}
