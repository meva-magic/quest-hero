using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private string slotId;
    
    public string GetSlotId() => slotId;
    
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            // Если слот пустой, перемещаем предмет сюда
            if (transform.childCount == 0)
            {
                dropped.transform.SetParent(transform);
                dropped.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Если слот занят, меняем предметы местами
                Transform currentItem = transform.GetChild(0);
                Transform previousParent = dropped.transform.parent;
                
                currentItem.SetParent(previousParent);
                currentItem.localPosition = Vector3.zero;
                
                dropped.transform.SetParent(transform);
                dropped.transform.localPosition = Vector3.zero;
            }
        }
    }
}
