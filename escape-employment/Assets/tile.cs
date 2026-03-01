using UnityEngine;

public class FloorTile : MonoBehaviour
{
    public int keyItemIndex;        // index of Key in ItemManager.items
    public Renderer tileRenderer;   // drag the tile's mesh renderer here

    private void Update()
    {
        int keyCount = ItemManager.Instance.GetCount(keyItemIndex);

        if (keyCount >= 1)
            tileRenderer.material.color = Color.green;
        else
            tileRenderer.material.color = Color.red;
    }
}

