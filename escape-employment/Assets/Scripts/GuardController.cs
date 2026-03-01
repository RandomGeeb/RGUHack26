using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour
{
    public enum GuardState { Patrol, Alert, Distracted, Lured }

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waypointWaitTime = 0.5f;

    [Header("Materials")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material patrolMaterial;
    [SerializeField] private Material alertMaterial;
    [SerializeField] private Material distractedMaterial;

    [Header("Room")]
    [SerializeField] public int RoomId = 0;   // 0 = Room_1 (Player 0), 1 = Room_2 (Player 1)

    [Header("FOV Reference")]
    [SerializeField] private FieldOfView fov;

    private NavMeshAgent _agent;
    private Animator _animator;
    private GuardState _state;
    private int _waypointIndex = 0;
    private float _waitTimer = 0f;
    private float _distractionTimer = 0f;
    private float _distractionDuration = 0f;

    // Lured path
    private Vector3[] _luredPath;
    private int _luredPathIndex = 0;
    private float _luredDuration = 0f;
    private float _luredTimer = 0f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = patrolSpeed;
        _agent.stoppingDistance = 0.1f;

        _animator = GetComponentInChildren<Animator>();
        if (_animator != null)
            _animator.applyRootMotion = false;
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
        if (_state == GuardState.Distracted) UpdateDistracted();
        if (_state == GuardState.Lured) UpdateLured();

        if (_animator != null)
            _animator.SetFloat(SpeedHash, _agent.desiredVelocity.magnitude);
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
        if (bodyRenderer == null) return;

        Material mat = newState switch
        {
            GuardState.Alert                  => alertMaterial,
            GuardState.Distracted or GuardState.Lured => distractedMaterial != null ? distractedMaterial : patrolMaterial,
            _                                 => patrolMaterial,
        };
        if (mat != null) bodyRenderer.material = mat;

        if (newState == GuardState.Alert)
            _agent.ResetPath();
    }

    public void OnDistracted(Vector3 position, float duration)
    {
        if (_state == GuardState.Alert) return;

        // Snap to nearest point on the NavMesh so the agent goes to the right spot
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            position = hit.position;

        Debug.Log($"[GuardController] {name} distracted for {duration}s — moving to {position}");
        _distractionDuration = duration;
        _distractionTimer = 0f;
        _agent.speed = patrolSpeed * 1.5f;
        _agent.SetDestination(position);
        SetState(GuardState.Distracted);
    }

    private void UpdateDistracted()
    {
        // Tick the timer even while moving — only pause if still pathfinding
        if (!_agent.pathPending)
            _distractionTimer += Time.deltaTime;

        if (_distractionTimer >= _distractionDuration)
        {
            _agent.speed = patrolSpeed;
            SetState(GuardState.Patrol);
            GoToNextWaypoint();
        }
    }

    /// <summary>Leads the guard through a specific path then lingers at the final point.</summary>
    public void FollowPath(Vector3[] path, float duration)
    {
        if (_state == GuardState.Alert || path == null || path.Length == 0) return;

        _luredPath = path;
        _luredPathIndex = 0;
        _luredDuration = duration;
        _luredTimer = 0f;

        _agent.speed = patrolSpeed * 1.5f;
        _agent.SetDestination(_luredPath[0]);
        SetState(GuardState.Lured);
        Debug.Log($"[GuardController] {name} lured — following {path.Length}-point path.");
    }

    private void UpdateLured()
    {
        if (_agent.pathPending) return;

        // Advance through path waypoints
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _luredPathIndex++;
            if (_luredPathIndex < _luredPath.Length)
            {
                _agent.SetDestination(_luredPath[_luredPathIndex]);
                return;
            }

            // Reached final point — linger then resume patrol
            _luredTimer += Time.deltaTime;
            if (_luredTimer >= _luredDuration)
            {
                _agent.speed = patrolSpeed;
                SetState(GuardState.Patrol);
                GoToNextWaypoint();
            }
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
