using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IDropHandler
{
    [Header("Prefabs")]
    [SerializeField]
    GameObject uiItemPrefab;

    [Header("References")]
    [SerializeField]
    Inventory inventory;
    [SerializeField]
    Transform uiInventoryParent;

    [Header("Drag & Drop")]
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private GraphicRaycaster raycaster;
    
    [Header("State")]
    [SerializeField]
    SerializedDictionary<string, GameObject> inventoryUI = new();
    
    private string draggingItemId = null;
    private GameObject draggingObject = null;
    private Vector2 dragOffset = Vector2.zero;

    private void Start()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (raycaster == null) raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (draggingObject != null && Input.GetMouseButton(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            draggingObject.transform.position = canvas.transform.TransformPoint(localPoint + dragOffset);
        }
        else if (draggingObject != null && !Input.GetMouseButton(0))
        {
            EndDrag();
        }
    }

    public void AddUIItem(string inventoryId, ItemObject item)
    {
        var itemUI = Instantiate(uiItemPrefab).GetComponent<ItemUI>();
        itemUI.transform.SetParent(uiInventoryParent);
        itemUI.transform.localScale = Vector3.one;
        inventoryUI.Add(inventoryId, itemUI.gameObject);
        
        // Добавляем компонент для перетаскивания
        var dragHandler = itemUI.gameObject.AddComponent<ItemDragHandler>();
        dragHandler.Initialize(this, inventoryId);
        
        itemUI.Initialize(inventoryId, item, inventory.DropItem);
    }

    public void RemoveUIItem(string inventoryId)
    {
        if (inventoryUI.TryGetValue(inventoryId, out GameObject itemUI))
        {
            inventoryUI.Remove(inventoryId);
            Destroy(itemUI);
        }
    }

    public void StartDrag(string inventoryId, GameObject dragObject)
    {
        draggingItemId = inventoryId;
        draggingObject = dragObject;
        
        // Сохраняем смещение для плавного перетаскивания
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        dragOffset = (Vector2)draggingObject.transform.localPosition - localPoint;
        
        // Поднимаем объект над остальными
        draggingObject.transform.SetAsLastSibling();
    }

    private void EndDrag()
    {
        if (draggingItemId == null || draggingObject == null) return;
        
        // Проверяем, куда был брошен предмет
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        bool droppedOnEmptySlot = false;
        string targetSlotId = null;
        
        foreach (var result in results)
        {
            // Ищем пустую ячейку инвентаря
            if (result.gameObject.CompareTag("InventorySlot") && result.gameObject.transform.childCount == 0)
            {
                // Нашли пустую ячейку
                droppedOnEmptySlot = true;
                targetSlotId = result.gameObject.name;
                break;
            }
        }
        
        if (droppedOnEmptySlot && targetSlotId != null)
        {
            // Перемещаем предмет в новую ячейку
            draggingObject.transform.SetParent(uiInventoryParent.Find(targetSlotId));
            draggingObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Возвращаем на исходное место
            draggingObject.transform.localPosition = Vector3.zero;
        }
        
        draggingItemId = null;
        draggingObject = null;
        dragOffset = Vector2.zero;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Реализация интерфейса IDropHandler
        if (draggingItemId != null)
        {
            EndDrag();
        }
    }
    
    public void SwapItems(string itemId1, string itemId2)
    {
        if (inventoryUI.TryGetValue(itemId1, out GameObject item1) && 
            inventoryUI.TryGetValue(itemId2, out GameObject item2))
        {
            Transform parent1 = item1.transform.parent;
            Transform parent2 = item2.transform.parent;
            
            item1.transform.SetParent(parent2);
            item2.transform.SetParent(parent1);
            
            item1.transform.localPosition = Vector3.zero;
            item2.transform.localPosition = Vector3.zero;
        }
    }
}

// Вспомогательный класс для перетаскивания предметов
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
        // Драг обрабатывается в InventoryUI.Update()
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Энд драг обрабатывается в InventoryUI
    }
}
