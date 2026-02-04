using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private InventoryUI inventoryUI;
    private string inventoryId;
    
    public void Initialize(InventoryUI ui, string id)
    {
        inventoryUI = ui;
        inventoryId = id;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.StartDrag(inventoryId, gameObject);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Drag is handled in InventoryUI.Update()
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // End drag is handled in InventoryUI
    }
}
