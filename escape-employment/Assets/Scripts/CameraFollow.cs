using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothSpeed = 4f;

    private Vector3 _roomTarget;

    private void Awake()
    {
        Instance = this;
        _roomTarget = transform.position;
    }

    public void SetRoomTarget(Vector3 position)
    {
        _roomTarget = position;
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _roomTarget, smoothSpeed * Time.deltaTime);
    }
}
