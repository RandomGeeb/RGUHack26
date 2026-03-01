using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject caughtPanel;
    [SerializeField] private GameObject escapedPanel;

    [Header("Win Condition")]
    [SerializeField] private ElevatorTrigger elevator0; // Player 1's elevator
    [SerializeField] private ElevatorTrigger elevator1; // Player 2's elevator
    [SerializeField] private int requiredKeys = 2;

    private bool _gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (caughtPanel  != null) caughtPanel.SetActive(false);
        if (escapedPanel != null) escapedPanel.SetActive(false);
    }

    public void CheckWinCondition()
    {
        if (_gameOver) return;

        bool e0 = elevator0 != null && elevator0.IsOccupied;
        bool e1 = elevator1 != null && elevator1.IsOccupied;
        int keys = CountKeys();

        Debug.Log($"[GameManager] WinCheck — Elevator0:{e0} Elevator1:{e1} Keys:{keys}/{requiredKeys}");

        if (!e0 || !e1) return;
        if (keys < requiredKeys) return;

        WinGame();
    }

    private int CountKeys()
    {
        if (ItemManager.Instance == null) return 0;
        int total = 0;
        for (int itemIndex = 0; itemIndex < ItemManager.Instance.Items.Count; itemIndex++)
        {
            if (!ItemManager.Instance.Items[itemIndex].isKey) continue;
            // Sum keys held by both players
            for (int p = 0; p < 2; p++)
                total += ItemManager.Instance.GetCountForPlayer(p, itemIndex);
        }
        return total;
    }

    private void WinGame()
    {
        _gameOver = true;
        if (escapedPanel != null) escapedPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayerReachedExit()
    {
        if (_gameOver) return;
        _gameOver = true;
        if (escapedPanel != null) escapedPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LoseGame()
    {
        if (_gameOver) return;
        _gameOver = true;
        if (caughtPanel != null) caughtPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (_gameOver && Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
