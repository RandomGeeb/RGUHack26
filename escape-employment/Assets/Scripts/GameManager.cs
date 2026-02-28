using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject caughtPanel;
    [SerializeField] private GameObject escapedPanel;

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

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
