using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IDropHandler
{
    [Header("Prefabs")]
    [SerializeField] GameObject uiItemPrefab;

    [Header("References")]
    [SerializeField] Inventory inventory;
    [SerializeField] Transform uiInventoryParent; // Parent that holds all slots

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryFullPanel; // Panel shown when inventory is full

    [Header("Slots")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>(); // Fixed slots
    [SerializeField] private int maxSlots = 6; // Maximum number of slots

    [Header("State")]
    [SerializeField] SerializedDictionary<string, GameObject> inventoryUI = new();
    
    private string draggingItemId = null;
    private GameObject draggingObject = null;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private CanvasGroup draggingCanvasGroup;

    private void Start()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (raycaster == null) raycaster = GetComponentInParent<GraphicRaycaster>();
        
        // Initialize slots if not manually assigned
        if (slots.Count == 0 && uiInventoryParent != null)
        {
            var foundSlots = uiInventoryParent.GetComponentsInChildren<InventorySlot>();
            if (foundSlots != null && foundSlots.Length > 0)
            {
                slots.AddRange(foundSlots);
            }
        }
        
        // Limit to maxSlots
        if (slots.Count > maxSlots)
        {
            slots = slots.GetRange(0, maxSlots);
        }
        
        // Hide full panel
        if (inventoryFullPanel != null)
            inventoryFullPanel.SetActive(false);
    }

    private void Update()
    {
        if (draggingObject != null && Input.GetMouseButton(0))
        {
            // Update position to follow cursor directly in screen space
            UpdateDragPosition();
        }
        else if (draggingObject != null && !Input.GetMouseButton(0))
        {
            EndDrag();
        }
    }

    private void UpdateDragPosition()
    {
        if (draggingObject == null) return;
        
        // Get the RectTransform of the dragging object
        RectTransform draggingRect = draggingObject.GetComponent<RectTransform>();
        if (draggingRect == null) return;
        
        // Convert mouse position to canvas position
        Vector2 mousePos = Input.mousePosition;
        
        // For Screen Space - Camera or World Space canvas
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
        {
            // Get the canvas RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            
            // Convert screen point to anchored position in canvas
            Vector2 anchoredPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                mousePos, 
                canvas.worldCamera, 
                out anchoredPos))
            {
                draggingRect.anchoredPosition = anchoredPos;
            }
        }
        else
        {
            // For Screen Space - Overlay or if no camera
            draggingRect.position = mousePos;
        }
    }

    public bool AddUIItem(string inventoryId, ItemObject item)
    {
        // Find first empty slot
        InventorySlot emptySlot = FindEmptySlot();
        
        if (emptySlot == null)
        {
            // Inventory is full
            ShowInventoryFullPanel();
            return false;
        }
        
        if (uiItemPrefab == null)
        {
            Debug.LogError("InventoryUI: uiItemPrefab is not assigned");
            return false;
        }
        
        var itemUI = Instantiate(uiItemPrefab).GetComponent<ItemUI>();
        if (itemUI == null)
        {
            Debug.LogError("InventoryUI: uiItemPrefab doesn't have ItemUI component");
            Destroy(itemUI?.gameObject);
            return false;
        }
        
        itemUI.transform.SetParent(emptySlot.transform);
        itemUI.transform.localPosition = Vector3.zero;
        itemUI.transform.localScale = Vector3.one;
        
        inventoryUI.Add(inventoryId, itemUI.gameObject);
        
        // Add drag handler
        var dragHandler = itemUI.gameObject.AddComponent<ItemDragHandler>();
        dragHandler.Initialize(this, inventoryId);
        
        itemUI.Initialize(inventoryId, item, inventory.DropItem);
        
        return true;
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
        
        // Make the item appear on top and follow cursor
        draggingObject.transform.SetParent(canvas.transform);
        draggingObject.transform.SetAsLastSibling();
        
        // Disable raycasts on dragging object so it doesn't block slot detection
        draggingCanvasGroup = draggingObject.GetComponent<CanvasGroup>();
        if (draggingCanvasGroup == null)
        {
            draggingCanvasGroup = draggingObject.AddComponent<CanvasGroup>();
        }
        draggingCanvasGroup.blocksRaycasts = false;
        
        // Update position immediately to cursor
        UpdateDragPosition();
    }

    private void EndDrag()
    {
        if (draggingItemId == null || draggingObject == null) return;
        
        // Re-enable raycasts
        if (draggingCanvasGroup != null)
        {
            draggingCanvasGroup.blocksRaycasts = true;
            draggingCanvasGroup = null;
        }
        
        // Check where the item was dropped
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        if (raycaster != null)
        {
            raycaster.Raycast(pointerData, results);
        }
        
        InventorySlot targetSlot = null;
        
        foreach (var result in results)
        {
            if (result.gameObject == null) continue;
            
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null && slots.Contains(slot))
            {
                targetSlot = slot;
                break;
            }
        }
        
        if (targetSlot != null)
        {
            // Try to move item to target slot
            if (targetSlot.transform.childCount == 0)
            {
                // Slot is empty, move item there
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Slot has item, swap items
                SwapItems(draggingItemId, targetSlot);
            }
        }
        else
        {
            // Dropped outside any slot - check if outside inventory panel
            if (!IsPointerOverInventoryUI())
            {
                // Drop the item from inventory
                if (inventory != null)
                    inventory.DropItem(draggingItemId);
            }
            else
            {
                // Return to original slot
                ReturnToOriginalSlot();
            }
        }
        
        draggingItemId = null;
        draggingObject = null;
    }

    private void SwapItems(string draggedItemId, InventorySlot targetSlot)
    {
        // Get the item currently in the target slot
        if (targetSlot.transform.childCount > 0)
        {
            Transform targetItem = targetSlot.transform.GetChild(0);
            ItemDragHandler targetDragHandler = targetItem.GetComponent<ItemDragHandler>();
            
            if (targetDragHandler != null)
            {
                // Find which item is in the target slot
                string targetItemId = null;
                foreach (var kvp in inventoryUI)
                {
                    if (kvp.Value == targetItem.gameObject)
                    {
                        targetItemId = kvp.Key;
                        break;
                    }
                }
                
                if (targetItemId != null)
                {
                    // Get original slot of dragged item
                    Transform originalParent = null;
                    foreach (var slot in slots)
                    {
                        if (slot.transform.childCount == 1 && slot.transform.GetChild(0).gameObject == draggingObject)
                        {
                            originalParent = slot.transform;
                            break;
                        }
                    }
                    
                    if (originalParent != null)
                    {
                        // Swap parents
                        draggingObject.transform.SetParent(targetSlot.transform);
                        draggingObject.transform.localPosition = Vector3.zero;
                        
                        targetItem.SetParent(originalParent);
                        targetItem.localPosition = Vector3.zero;
                    }
                }
            }
        }
    }

    private bool IsPointerOverInventoryUI()
    {
        if (raycaster == null) return false;
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject == null) continue;
            
            if (result.gameObject == gameObject || 
                result.gameObject.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    private void ReturnToOriginalSlot()
    {
        if (draggingObject == null) return;
        
        // Find which slot originally contained this item
        foreach (var slot in slots)
        {
            if (slot.transform.childCount == 0)
            {
                // Check if this slot originally had our item by looking at parent-child relationship
                bool foundInInventory = false;
                foreach (var kvp in inventoryUI)
                {
                    if (kvp.Value == draggingObject)
                    {
                        foundInInventory = true;
                        break;
                    }
                }
                
                if (foundInInventory)
                {
                    draggingObject.transform.SetParent(slot.transform);
                    draggingObject.transform.localPosition = Vector3.zero;
                    return;
                }
            }
        }
        
        // If we can't find the original slot, just put it in the first empty one
        InventorySlot emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            draggingObject.transform.SetParent(emptySlot.transform);
            draggingObject.transform.localPosition = Vector3.zero;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // If something is dropped on the inventory panel itself
        if (draggingItemId != null)
        {
            EndDrag();
        }
    }

    private InventorySlot FindEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.transform.childCount == 0)
                return slot;
        }
        return null;
    }

    private void ShowInventoryFullPanel()
    {
        if (inventoryFullPanel != null)
        {
            inventoryFullPanel.SetActive(true);
            // Auto-hide after 2 seconds
            Invoke(nameof(HideInventoryFullPanel), 2f);
        }
    }

    private void HideInventoryFullPanel()
    {
        if (inventoryFullPanel != null)
            inventoryFullPanel.SetActive(false);
    }
    
    public bool HasEmptySlot()
    {
        return FindEmptySlot() != null;
    }
    
    private void OnDestroy()
    {
        // Clean up any pending invokes
        CancelInvoke(nameof(HideInventoryFullPanel));
    }
}
