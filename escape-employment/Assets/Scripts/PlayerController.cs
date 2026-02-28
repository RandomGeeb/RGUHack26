using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private LayerMask groundMask = 1; // Default layer

    private NavMeshAgent _agent;
    private Camera _cam;
    private bool _isActive = true;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = moveSpeed;
        _agent.angularSpeed = 720f;
        _agent.acceleration = 16f;
        _agent.stoppingDistance = 0.1f;
    }

    private void Start() => _cam = Camera.main;

    public void SetActive(bool active)
    {
        _isActive = active;
        if (!active) _agent.ResetPath();
    }

    private void Update()
    {
        if (!_isActive) return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask))
                _agent.SetDestination(hit.point);
        }
    }
}
