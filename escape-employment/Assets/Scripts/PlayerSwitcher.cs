using UnityEngine;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher Instance { get; private set; }
    public PlayerController ActivePlayer => players[_currentIndex];

    [SerializeField] private PlayerController[] players;
    [SerializeField] private Transform[] roomPlanes;
    [SerializeField] private Vector3 offset = new Vector3(0f, 22f, -5f);

    private int _currentIndex = 0;

    private void Awake() => Instance = this;

    private void Start()
    {
        for (int i = 0; i < players.Length; i++)
            players[i].SetActive(i == 0);

        UpdateCamera();
        ItemManager.Instance?.SetActivePlayer(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SwitchPlayer();
    }

    private void SwitchPlayer()
    {
        players[_currentIndex].SetActive(false);
        _currentIndex = (_currentIndex + 1) % players.Length;
        players[_currentIndex].SetActive(true);
        UpdateCamera();
        ItemManager.Instance?.SetActivePlayer(_currentIndex);
    }

    private void UpdateCamera()
    {
        CameraFollow.Instance.SetTarget(players[_currentIndex].transform);

        if (_currentIndex < roomPlanes.Length && roomPlanes[_currentIndex] != null)
        {
            Renderer r = roomPlanes[_currentIndex].GetComponent<Renderer>();
            if (r != null)
                CameraFollow.Instance.SetBounds(r.bounds.min + offset, r.bounds.max + offset);
        }

        CameraFollow.Instance.offset = offset;
    }
}
