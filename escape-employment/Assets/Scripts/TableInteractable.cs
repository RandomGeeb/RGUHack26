using UnityEngine;

public class TableInteractable : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private float freezeDuration = 2f;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.V)) return;

        Transform player = GetActivePlayer();
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) > interactRange) return;

        FreezeNearbyGuards();
    }

    private void FreezeNearbyGuards()
    {
        GuardController[] guards = (GuardController[])Object.FindObjectsOfType(typeof(GuardController));
        foreach (GuardController guard in guards)
        {
            Debug.Log($"[Table] Freezing '{guard.name}' for {freezeDuration}s.");
            guard.Freeze(freezeDuration);
        }
    }

    private Transform GetActivePlayer()
    {
        if (PlayerSwitcher.Instance != null)
            return PlayerSwitcher.Instance.ActivePlayer.transform;
        GameObject p = GameObject.FindWithTag("Player");
        return p != null ? p.transform : null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
