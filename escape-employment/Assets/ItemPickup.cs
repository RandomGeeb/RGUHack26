using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public int itemIndex; // This matches the index in ItemManager.items

    private void OnMouseDown()
    {
        ItemManager.Instance.AddItem(itemIndex, 1);
        Destroy(gameObject);
    }
}
