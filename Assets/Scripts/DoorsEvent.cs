using UnityEngine;

public class DoorsEvent : MonoBehaviour
{
    [SerializeField] private GameObject entranceDoor;
    [SerializeField] private Animator animatorEntranceDoor;
    private int _idOpenDoor;

    private void OnEnable()
    {
        _idOpenDoor = Animator.StringToHash("Open");
        entranceDoor = GameObject.FindGameObjectWithTag("EntranceDoor");
        animatorEntranceDoor = entranceDoor.GetComponent<Animator>();
    }

    public void DoorOut()
    {
        animatorEntranceDoor.SetTrigger(_idOpenDoor);
    }
}
