using Unity.Cinemachine;
using UnityEngine;

public class CustomCameraOffset : MonoBehaviour
{
    public CinemachineCamera CinemachineCamera;
    public CinemachinePositionComposer PositionComposer;

    private void Start()
    {
        PositionComposer = CinemachineCamera.GetComponent<CinemachinePositionComposer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PositionComposer.TargetOffset.y = -1.8f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PositionComposer.TargetOffset.y = 0;
    }
}
