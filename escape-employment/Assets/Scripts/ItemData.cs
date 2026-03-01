using UnityEngine;

public enum ItemType { Noise, Decoy }

[CreateAssetMenu(fileName = "NewItem", menuName = "Stealth/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int count = 3;
    public ItemType type;
    public float distractionDuration = 5f;
    public float throwDistance = 8f;
}
