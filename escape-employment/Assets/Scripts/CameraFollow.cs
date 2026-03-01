using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private float smoothSpeed = 4f;
    [SerializeField] public Vector3 offset = new Vector3(0f, 22f, -5f);

    private Transform _target;
    private bool _hasBounds;
    private Vector3 _boundsMin;
    private Vector3 _boundsMax;

    private void Awake()
    {
        Instance = this;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetBounds(Vector3 min, Vector3 max)
    {
        _boundsMin = min;
        _boundsMax = max;
        _hasBounds = true;
    }

    // Keep legacy support for CameraZone panning
    public void SetRoomTarget(Vector3 _)
    {
        _hasBounds = false;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = _target.position + offset;

        if (_hasBounds)
        {
            desired.x = Mathf.Clamp(desired.x, _boundsMin.x, _boundsMax.x);
            desired.z = Mathf.Clamp(desired.z, _boundsMin.z, _boundsMax.z);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
