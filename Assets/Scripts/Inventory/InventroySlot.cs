using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private string slotId;
    
    public string GetSlotId() => slotId;
    
    public void OnDrop(PointerEventData eventData)
    {
        // This is now handled by InventoryUI
    }
}
