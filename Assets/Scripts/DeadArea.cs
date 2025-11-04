using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadArea : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        player = other.gameObject.GetComponent<PlayerController>();
        if (other.CompareTag("Player")) player.Die();
    }   
}
