using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image image;
    [SerializeField]
    Button button;
    [SerializeField]
    TextMeshProUGUI itemNameText;
    
    private string inventoryId;
    private ItemObject item;
    private Action<string> removeItemAction;

    public void Initialize(string inventoryId, ItemObject item, Action<string> removeItemAction)
    {
        this.inventoryId = inventoryId;
        this.item = item;
        this.removeItemAction = removeItemAction;
        
        image.sprite = item.icon;
        transform.localScale = Vector3.one;
        
        if (itemNameText != null)
            itemNameText.text = item.name;
            
        button.onClick.AddListener(() => removeItemAction.Invoke(inventoryId));
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    
    public string GetInventoryId() => inventoryId;
    public ItemObject GetItem() => item;
}
