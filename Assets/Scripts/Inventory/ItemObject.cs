using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "RumpledCode/Item", order = 1)]
public class ItemObject : ScriptableObject
{
    public string id;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public bool isQuestItem; // Added to identify quest items
}
