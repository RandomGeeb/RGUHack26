using UnityEngine;

public class GreenOnClick : MonoBehaviour
{
    [SerializeField] private string requiredItemName = "Key";

    void OnMouseDown()
    {
        // 1. Find the item index
        int index = ItemManager.Instance.Items.FindIndex(i => i.itemName == requiredItemName);
        if (index < 0)
        {
            Debug.LogWarning($"Item '{requiredItemName}' not found in ItemManager.Items.");
            return;
        }

        // 2. Check if active player has at least one
        int count = ItemManager.Instance.GetCount(index);
        Debug.Log($"Checking key: index={index}, count={count}");

        if (count > 0)
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = Color.green;
                Debug.Log("Tile turned green.");
            }
        }
        else
        {
            Debug.Log("Player does NOT have the required key.");
        }
    }
}
