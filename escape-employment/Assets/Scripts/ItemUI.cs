using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    public static ItemUI Instance { get; private set; }

    // No "panel" field needed — ItemUI lives ON the panel, so we use gameObject directly.

    [Header("Item Slots (bottom bar)")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("Description Panel")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private List<GameObject> _slots = new List<GameObject>();
    private bool _slotsBuilt = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildSlots();       // build slots first while object is active
        _slotsBuilt = true;
        gameObject.SetActive(false);  // hide until F is held
    }

    private void BuildSlots()
    {
        foreach (GameObject s in _slots) Destroy(s);
        _slots.Clear();

        if (ItemManager.Instance == null || slotPrefab == null || slotsParent == null) return;

        var items = ItemManager.Instance.Items;
        for (int i = 0; i < items.Count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsParent);
            _slots.Add(slot);
            RefreshSlot(i, i == 0);
        }
    }

    private void RefreshSlot(int index, bool selected)
    {
        if (index >= _slots.Count) return;
        if (ItemManager.Instance == null || index >= ItemManager.Instance.Items.Count) return;
        GameObject slot = _slots[index];
        ItemData item = ItemManager.Instance.Items[index];

        Image bg = slot.GetComponent<Image>();
        if (bg != null)
            bg.color = selected ? new Color(0.5f, 0.45f, 0.05f, 0.95f)
                                : new Color(0.1f, 0.1f, 0.1f, 0.9f);

        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.color  = item.icon != null ? Color.white : Color.clear;
        }

        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.text = item.itemName.ToUpper();

        TextMeshProUGUI countText = slot.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
        if (countText != null)
        {
            int count = ItemManager.Instance.GetCount(index);
            countText.text = count > 0 ? count.ToString() : "-";
        }
    }

    public void Show()
    {
        if (!_slotsBuilt) BuildSlots();
        gameObject.SetActive(true);
        UpdateSelection(ItemManager.Instance.SelectedIndex);
    }

    public void Hide() => gameObject.SetActive(false);

    public void UpdateSelection(int index)
    {
        for (int i = 0; i < _slots.Count; i++)
            RefreshSlot(i, i == index);

        if (itemNameText == null || descriptionText == null) return;

        var items = ItemManager.Instance.Items;
        if (index < items.Count)
        {
            itemNameText.text   = items[index].itemName.ToUpper();
            descriptionText.text = items[index].description;
        }
    }
}
