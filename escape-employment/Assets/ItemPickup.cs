using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // This should match the index of the item in ItemManager.items
    public int itemIndex;

    private void OnMouseDown()
    {
        ItemManager.Instance.AddItem(itemIndex, 1);
        Destroy(gameObject);
    }
}
