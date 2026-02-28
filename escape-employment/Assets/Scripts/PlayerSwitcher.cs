using UnityEngine;

public class PlayerSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerController[] players;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 22f, -5f);

    private int _currentIndex = 0;

    private void Start()
    {
        for (int i = 0; i < players.Length; i++)
            players[i].SetActive(i == 0);

        PanToCurrentPlayer();
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
        PanToCurrentPlayer();
    }

    private void PanToCurrentPlayer()
    {
        Vector3 target = players[_currentIndex].transform.position + cameraOffset;
        CameraFollow.Instance.SetRoomTarget(target);
    }
}
