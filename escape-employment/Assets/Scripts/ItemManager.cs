using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [SerializeField] private List<ItemData> items;
    [SerializeField] private GameObject thrownItemPrefab;

    private ItemUI UI => ItemUI.Instance;

    private int _selectedIndex = 0;
    private bool _menuOpen = false;
    private int _activePlayer = 0;

    // Per-player item counts — [playerIndex][itemIndex]
    private List<int>[] _counts;

    public List<ItemData> Items => items;
    public int SelectedIndex => _selectedIndex;

    private void Awake()
    {
        // Destroy duplicate — can happen if a stale instance persisted
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Init counts here so they are ready before any Start() runs
        _counts = new List<int>[2];
        for (int p = 0; p < 2; p++)
        {
            _counts[p] = new List<int>();
            foreach (var item in items)
                _counts[p].Add(item.count);
        }
    }

    /// <summary>Called by PlayerSwitcher whenever the active player changes.</summary>
    public void SetActivePlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _counts.Length) return;
        _activePlayer = playerIndex;
        if (_menuOpen) UI.UpdateSelection(_selectedIndex);
    }

    /// <summary>Returns a specific player's remaining count for an item slot.</summary>
    public int GetCountForPlayer(int playerIndex, int itemIndex)
    {
        if (_counts == null || playerIndex >= _counts.Length
            || itemIndex < 0 || itemIndex >= _counts[playerIndex].Count) return 0;
        return _counts[playerIndex][itemIndex];
    }

    /// <summary>Returns the active player's remaining count for an item slot.</summary>
    public int GetCount(int itemIndex)
    {
        if (_counts == null
            || _activePlayer >= _counts.Length
            || itemIndex < 0
            || itemIndex >= _counts[_activePlayer].Count)
            return 0;
        return _counts[_activePlayer][itemIndex];
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            if (!_menuOpen)
            {
                _menuOpen = true;
                UI.Show();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow)) CycleItem(1);
            if (Input.GetKeyDown(KeyCode.LeftArrow))  CycleItem(-1);
            if (Input.GetKeyDown(KeyCode.E))           ThrowSelected();
        }
        else if (_menuOpen)
        {
            _menuOpen = false;
            UI.Hide();
        }
    }

    private void CycleItem(int dir)
    {
        if (items.Count == 0) return;
        _selectedIndex = (_selectedIndex + dir + items.Count) % items.Count;
        UI.UpdateSelection(_selectedIndex);
    }

    private void ThrowSelected()
    {
        if (items.Count == 0) return;
        if (GetCount(_selectedIndex) <= 0) return;

        // Check prefab BEFORE consuming the item
        if (thrownItemPrefab == null)
        {
            Debug.LogError("[ItemManager] Thrown Item Prefab is not assigned! Drag a prefab with ThrownItem.cs into the Inspector.");
            return;
        }

        _counts[_activePlayer][_selectedIndex]--;
        if (UI != null) UI.UpdateSelection(_selectedIndex);

        Transform player = PlayerSwitcher.Instance.ActivePlayer.transform;
        ItemData item = items[_selectedIndex];

        // Project mouse cursor onto the ground plane (y = 0)
        Vector3 throwPos = GetMouseWorldPos();
        if (throwPos == Vector3.zero)
            throwPos = player.position + Vector3.forward * item.throwDistance;

        // Clamp to max throw distance from player
        Vector3 toTarget = throwPos - player.position;
        toTarget.y = 0f;
        if (toTarget.magnitude > item.throwDistance)
            throwPos = player.position + toTarget.normalized * item.throwDistance;
        throwPos.y = 0f;

        // Spawn at landing spot; ThrownItem will animate travel from player
        GameObject go = Instantiate(thrownItemPrefab, throwPos, Quaternion.identity);
        ThrownItem thrown = go.GetComponent<ThrownItem>();
        if (thrown == null)
        {
            Debug.LogError("[ItemManager] ThrownItem prefab is missing the ThrownItem script!");
            Destroy(go);
            return;
        }

        Debug.Log($"[ItemManager] Throwing '{item.itemName}' to {throwPos}, player at {player.position}");
        thrown.Init(item.distractionDuration, player.position, _activePlayer);
    }

    private Vector3 GetMouseWorldPos()
    {
        if (Camera.main == null) return Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float dist))
            return ray.GetPoint(dist);
        return Vector3.zero;
    }

    public void AddItem(int itemIndex, int amount)
    {
        if (itemIndex < 0 || itemIndex >= items.Count) return;

        ItemData item = items[itemIndex];
        int current = _counts[_activePlayer][itemIndex];

        // Enforce per-player max carry
        if (current >= item.maxCarry)
        {
            Debug.Log($"Player {_activePlayer} already has the maximum of {item.itemName} ({item.maxCarry}).");
            return;
        }

        int newAmount = Mathf.Min(current + amount, item.maxCarry);
        _counts[_activePlayer][itemIndex] = newAmount;

        if (_menuOpen)
            UI.UpdateSelection(_selectedIndex);

        Debug.Log($"Player {_activePlayer} now has {newAmount} of {item.itemName}");

        if (item.isKey)
            KeyPickupPopup.Instance?.Show($"KEY OBTAINED");
    }




}
