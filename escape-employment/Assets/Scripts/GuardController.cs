using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    public enum GuardState { Patrol, Alert }

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waypointWaitTime = 0.5f;

    [Header("Materials")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material patrolMaterial;
    [SerializeField] private Material alertMaterial;

    [Header("FOV Reference")]
    [SerializeField] private FieldOfView fov;

    private NavMeshAgent _agent;
    private GuardState _state;
    private int _waypointIndex = 0;
    private float _waitTimer = 0f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = patrolSpeed;
    }

    private void Start()
    {
        if (fov != null) fov.OnPlayerSpotted += HandlePlayerSpotted;
        SetState(GuardState.Patrol);
        GoToNextWaypoint();
    }

    private void OnDestroy()
    {
        if (fov != null) fov.OnPlayerSpotted -= HandlePlayerSpotted;
    }

    private void Update()
    {
        if (_state == GuardState.Patrol) UpdatePatrol();
    }

    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0 || _agent.pathPending) return;
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waypointWaitTime)
            {
                _waitTimer = 0f;
                _waypointIndex = (_waypointIndex + 1) % waypoints.Length;
                GoToNextWaypoint();
            }
        }
    }

    private void SetState(GuardState newState)
    {
        _state = newState;
        if (newState == GuardState.Alert)
        {
            _agent.ResetPath();
            if (bodyRenderer != null && alertMaterial != null)
                bodyRenderer.material = alertMaterial;
        }
        else
        {
            if (bodyRenderer != null && patrolMaterial != null)
                bodyRenderer.material = patrolMaterial;
        }
    }

    private void HandlePlayerSpotted()
    {
        if (_state == GuardState.Alert) return;
        SetState(GuardState.Alert);
        GameManager.Instance.LoseGame();
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_waypointIndex].position);
    }
}
