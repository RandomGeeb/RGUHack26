using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [Tooltip("0 = Player 1's elevator, 1 = Player 2's elevator")]
    [SerializeField] private int playerIndex = 0;

    public bool IsOccupied { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Elevator {playerIndex}] OnTriggerEnter: {other.gameObject.name} tag={other.tag}");
        if (IsCorrectPlayer(other))
        {
            IsOccupied = true;
            Debug.Log($"[Elevator {playerIndex}] Correct player entered. Checking win...");
            GameManager.Instance.CheckWinCondition();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsCorrectPlayer(other))
        {
            IsOccupied = false;
            Debug.Log($"[Elevator {playerIndex}] Player exited.");
        }
    }

    // Also check every frame while inside in case Enter fired before setup was ready
    private void OnTriggerStay(Collider other)
    {
        if (!IsOccupied && IsCorrectPlayer(other))
        {
            IsOccupied = true;
            GameManager.Instance.CheckWinCondition();
        }
    }

    private bool IsCorrectPlayer(Collider other)
    {
        if (!other.CompareTag("Player")) return false;
        return GetPlayerIndex(other) == playerIndex;
    }

    private int GetPlayerIndex(Collider other)
    {
        if (PlayerSwitcher.Instance == null) return -1;
        GameObject[] players = PlayerSwitcher.Instance.Players;
        for (int i = 0; i < players.Length; i++)
            if (players[i] == other.gameObject) return i;
        return -1;
    }
}
