using UnityEngine;

public class ArrowController : TrampolineController
{
    [Header("Additional Info")]
    [SerializeField] private float cooldown;
    [SerializeField] private bool rotationRight;
    [SerializeField] private float rotationSpeed = 120;
    private int _direction = -1;

    private void Update()
    {
        _direction = rotationRight ? -1 : 1;
        transform.Rotate(0, 0, (rotationSpeed * _direction) * Time.deltaTime);
    }

    private void DestroyMe()
    {
        GameObject arrowPrefab = GameManager.Instance.arrowPrefab;
        GameManager.Instance.CreateObject(arrowPrefab, transform, cooldown);
        Destroy(gameObject);
    } 
}
