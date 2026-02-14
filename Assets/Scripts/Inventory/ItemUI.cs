using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ItemUI : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Button button;
    [SerializeField]
    TextMeshProUGUI itemNameText;
    
    [Header("Appearance Settings")]
    [SerializeField]
    Color defaultTint = Color.white;
    
    private string inventoryId;
    private ItemObject item;
    private Action<string> removeItemAction;

    public void Initialize(string inventoryId, ItemObject item, Action<string> removeItemAction)
    {
        this.inventoryId = inventoryId;
        this.item = item;
        this.removeItemAction = removeItemAction;
        
        image.sprite = item.icon;
        image.color = defaultTint; // Apply the tint from inspector
        
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
    
    public void SetTint(Color newTint)
    {
        image.color = newTint;
    }
}
