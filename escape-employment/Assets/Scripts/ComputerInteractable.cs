using UnityEngine;

public class ComputerInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 2f;      // how close player must be to press V
    [SerializeField] private float guardNotifyRadius = 15f; // how far guards can hear it
    [SerializeField] private float distractionDuration = 5f;

    [Header("Guard Destination")]
    [Tooltip("Where the guard stands when it arrives. Place an empty GO in front of the computer.")]
    [SerializeField] private Transform standPosition;

    [Header("Guard Path")]
    [Tooltip("Waypoints the guard follows IN ORDER before arriving. Leave empty to go directly.")]
    [SerializeField] private Transform[] guardPath;

    [Header("Room")]
    [SerializeField] private int roomId = 0; // match guard's RoomId

    private Transform _playerTransform;

    private void Start()
    {
        // Find the active player via PlayerSwitcher if available, else find by tag
        if (PlayerSwitcher.Instance != null)
            _playerTransform = PlayerSwitcher.Instance.ActivePlayer.transform;
        else
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) _playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.V)) return;

        // Refresh active player in case they switched
        if (PlayerSwitcher.Instance != null)
            _playerTransform = PlayerSwitcher.Instance.ActivePlayer.transform;

        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist > interactRange) return;

        AlertNearbyGuards();
    }

    private void AlertNearbyGuards()
    {
        // Build the full path: user-defined waypoints + computer position as final point
        Vector3[] path = BuildPath();

        GuardController[] guards = (GuardController[])Object.FindObjectsOfType(typeof(GuardController));
        foreach (GuardController guard in guards)
        {
            if (guard.RoomId != roomId) continue;

            float dist = Vector3.Distance(transform.position, guard.transform.position);
            if (dist <= guardNotifyRadius)
            {
                Debug.Log($"[Computer] Luring '{guard.name}' via {path.Length}-point path.");
                guard.FollowPath(path, distractionDuration);
            }
        }
    }

    private Vector3[] BuildPath()
    {
        int extra = (guardPath != null) ? guardPath.Length : 0;
        Vector3[] path = new Vector3[extra + 1];
        for (int i = 0; i < extra; i++)
            path[i] = guardPath[i].position;
        // Use standPosition if assigned, otherwise fall back to this object's position
        path[extra] = standPosition != null ? standPosition.position : transform.position;
        return path;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, guardNotifyRadius);

        if (standPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(standPosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, standPosition.position);
        }
    }
}
