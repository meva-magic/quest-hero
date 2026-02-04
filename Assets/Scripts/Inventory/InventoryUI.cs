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
    [SerializeField] Transform uiInventoryParent;

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryFullPanel;
    [SerializeField] private Button inventoryFullCloseButton;

    [Header("Slots")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 3;

    [Header("State")]
    [SerializeField] SerializedDictionary<string, GameObject> inventoryUI = new();
    
    private string draggingItemId = null;
    private GameObject draggingObject = null;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private CanvasGroup draggingCanvasGroup;
    private Transform originalParent;

    private void Start()
    {
        // Get canvas references
        canvas = GetComponentInParent<Canvas>();
        raycaster = GetComponentInParent<GraphicRaycaster>();
        
        Debug.Log($"InventoryUI Start - Panel assigned: {(inventoryFullPanel != null ? inventoryFullPanel.name : "NULL")}");
        
        // Initialize slots
        InitializeSlots();
        
        // Initialize inventory full panel
        InitializeInventoryFullPanel();
    }

    private void InitializeSlots()
    {
        // If slots not manually assigned, find them
        if (slots.Count == 0 && uiInventoryParent != null)
        {
            var foundSlots = uiInventoryParent.GetComponentsInChildren<InventorySlot>();
            if (foundSlots != null)
            {
                slots.AddRange(foundSlots);
                Debug.Log($"Found {slots.Count} slots automatically");
            }
        }
        
        // Limit to maxSlots
        if (slots.Count > maxSlots)
        {
            slots = slots.GetRange(0, maxSlots);
            Debug.Log($"Limited slots to {maxSlots}");
        }
        
        Debug.Log($"Total slots: {slots.Count}");
    }

    private void InitializeInventoryFullPanel()
    {
        if (inventoryFullPanel == null)
        {
            Debug.LogError("CRITICAL ERROR: inventoryFullPanel is NULL! Assign it in the Inspector.");
            return;
        }
        
        // Make sure panel starts disabled
        inventoryFullPanel.SetActive(false);
        Debug.Log($"Panel initialized: {inventoryFullPanel.name}, Initially disabled");
        
        // Setup close button
        if (inventoryFullCloseButton != null)
        {
            inventoryFullCloseButton.onClick.AddListener(() => 
            {
                if (inventoryFullPanel != null)
                {
                    inventoryFullPanel.SetActive(false);
                    Debug.Log("Panel closed manually via button");
                }
            });
        }
    }

    private void Update()
    {
        // TEST: Press T to manually show panel
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("=== MANUAL TEST: T key pressed ===");
            ShowInventoryFullPanel();
        }
        
        // Press Y to check panel state
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DebugPanelState();
        }
        
        // Drag logic
        if (draggingObject != null && Input.GetMouseButton(0))
        {
            UpdateDragPosition();
        }
        else if (draggingObject != null && !Input.GetMouseButton(0))
        {
            EndDrag();
        }
    }

    private void DebugPanelState()
    {
        if (inventoryFullPanel == null)
        {
            Debug.Log("Panel: NULL");
            return;
        }
        
        Debug.Log($"=== PANEL STATE ===");
        Debug.Log($"Name: {inventoryFullPanel.name}");
        Debug.Log($"ActiveSelf: {inventoryFullPanel.activeSelf}");
        Debug.Log($"ActiveInHierarchy: {inventoryFullPanel.activeInHierarchy}");
        Debug.Log($"Parent: {inventoryFullPanel.transform.parent?.name}");
        
        // Check RectTransform
        RectTransform rt = inventoryFullPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"Position: {rt.anchoredPosition}");
            Debug.Log($"Size: {rt.sizeDelta}");
            Debug.Log($"Scale: {rt.localScale}");
        }
        
        // Check Image
        Image img = inventoryFullPanel.GetComponent<Image>();
        if (img != null)
        {
            Debug.Log($"Image color: {img.color}, Alpha: {img.color.a}");
        }
    }

    private void UpdateDragPosition()
    {
        if (draggingObject == null || canvas == null) return;
        
        RectTransform draggingRect = draggingObject.GetComponent<RectTransform>();
        if (draggingRect == null) return;
        
        Vector2 mousePos = Input.mousePosition;
        
        // Different handling for different canvas render modes
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            draggingRect.position = mousePos;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
        {
            Vector3 worldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, canvas.planeDistance));
            draggingRect.position = worldPos;
            draggingRect.rotation = canvas.transform.rotation;
        }
    }

    public bool AddUIItem(string inventoryId, ItemObject item)
    {
        Debug.Log($"AddUIItem called - Item: {item?.name}, ID: {inventoryId}");
        
        // Find first empty slot
        InventorySlot emptySlot = FindEmptySlot();
        
        if (emptySlot == null)
        {
            Debug.Log("NO EMPTY SLOTS - Inventory is FULL!");
            ShowInventoryFullPanel();
            return false;
        }
        
        // Validate prefab
        if (uiItemPrefab == null)
        {
            Debug.LogError("Cannot add item: uiItemPrefab is null!");
            return false;
        }
        
        if (item == null)
        {
            Debug.LogError("Cannot add item: ItemObject is null!");
            return false;
        }
        
        // Instantiate UI item
        GameObject itemObj = Instantiate(uiItemPrefab);
        ItemUI itemUI = itemObj.GetComponent<ItemUI>();
        
        if (itemUI == null)
        {
            Debug.LogError("uiItemPrefab doesn't have ItemUI component!");
            Destroy(itemObj);
            return false;
        }
        
        // Set parent and position
        itemObj.transform.SetParent(emptySlot.transform);
        itemObj.transform.localPosition = Vector3.zero;
        itemObj.transform.localScale = Vector3.one;
        
        // Store reference
        inventoryUI.Add(inventoryId, itemObj);
        
        // Add drag handler
        ItemDragHandler dragHandler = itemObj.AddComponent<ItemDragHandler>();
        dragHandler.Initialize(this, inventoryId);
        
        // Initialize UI
        itemUI.Initialize(inventoryId, item, inventory.DropItem);
        
        Debug.Log($"Item added successfully to slot: {emptySlot.name}");
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
        originalParent = dragObject.transform.parent;
        
        // Bring to front
        dragObject.transform.SetAsLastSibling();
        
        // Disable raycasts during drag
        draggingCanvasGroup = draggingObject.GetComponent<CanvasGroup>();
        if (draggingCanvasGroup == null)
            draggingCanvasGroup = draggingObject.AddComponent<CanvasGroup>();
        draggingCanvasGroup.blocksRaycasts = false;
        
        // Update position immediately
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
        
        // Check where item was dropped
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        if (raycaster != null)
            raycaster.Raycast(pointerData, results);
        
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
            // Target slot found
            if (targetSlot.transform.childCount == 0)
            {
                // Slot is empty, move item there
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
                Debug.Log($"Item moved to empty slot: {targetSlot.name}");
            }
            else
            {
                // Slot has item, swap them
                SwapItems(draggingItemId, targetSlot);
                Debug.Log($"Items swapped with slot: {targetSlot.name}");
            }
        }
        else
        {
            // No slot found - check if dropped outside inventory
            if (!IsPointerOverInventoryUI())
            {
                // Dropped outside inventory - drop item
                if (inventory != null)
                {
                    inventory.DropItem(draggingItemId);
                    Debug.Log("Item dropped outside inventory");
                }
            }
            else
            {
                // Dropped on inventory UI but not on a slot - return to original
                ReturnToOriginalPosition();
                Debug.Log("Item returned to original position");
            }
        }
        
        draggingItemId = null;
        draggingObject = null;
        originalParent = null;
    }

    private void SwapItems(string draggedItemId, InventorySlot targetSlot)
    {
        if (targetSlot.transform.childCount > 0 && originalParent != null)
        {
            Transform targetItem = targetSlot.transform.GetChild(0);
            
            // Swap parents
            draggingObject.transform.SetParent(targetSlot.transform);
            draggingObject.transform.localPosition = Vector3.zero;
            
            targetItem.SetParent(originalParent);
            targetItem.localPosition = Vector3.zero;
        }
    }

    private bool IsPointerOverInventoryUI()
    {
        if (raycaster == null || EventSystem.current == null) return false;
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject == null) continue;
            
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                return true;
        }
        return false;
    }

    private void ReturnToOriginalPosition()
    {
        if (draggingObject != null && originalParent != null)
        {
            draggingObject.transform.SetParent(originalParent);
            draggingObject.transform.localPosition = Vector3.zero;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggingItemId != null)
            EndDrag();
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

    public void ShowInventoryFullPanel()
    {
        if (inventoryFullPanel == null)
        {
            Debug.LogError("ShowInventoryFullPanel: Panel is NULL!");
            return;
        }
        
        Debug.Log($"=== SHOWING INVENTORY FULL PANEL ===");
        Debug.Log($"Panel: {inventoryFullPanel.name}");
        Debug.Log($"Before - ActiveSelf: {inventoryFullPanel.activeSelf}, ActiveInHierarchy: {inventoryFullPanel.activeInHierarchy}");
        
        // Activate the panel
        inventoryFullPanel.SetActive(true);
        
        // Bring to front
        inventoryFullPanel.transform.SetAsLastSibling();
        
        // Force canvas update
        if (canvas != null)
        {
            Canvas.ForceUpdateCanvases();
        }
        
        Debug.Log($"After - ActiveSelf: {inventoryFullPanel.activeSelf}, ActiveInHierarchy: {inventoryFullPanel.activeInHierarchy}");
        
        // Check if panel has proper components
        CheckPanelVisibility();
        
        // Auto-hide after 3 seconds
        CancelInvoke(nameof(HideInventoryFullPanel));
        Invoke(nameof(HideInventoryFullPanel), 3f);
    }

    private void CheckPanelVisibility()
    {
        if (inventoryFullPanel == null) return;
        
        // Check if panel has Image component
        Image image = inventoryFullPanel.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning("Panel has no Image component - may be invisible!");
        }
        else if (image.color.a <= 0.1f)
        {
            Debug.LogWarning($"Panel Image alpha is very low ({image.color.a}) - may be transparent!");
        }
        
        // Check scale
        if (inventoryFullPanel.transform.localScale.magnitude < 0.1f)
        {
            Debug.LogWarning("Panel scale is very small - may be invisible!");
        }
    }

    private void HideInventoryFullPanel()
    {
        if (inventoryFullPanel != null && inventoryFullPanel.activeSelf)
        {
            inventoryFullPanel.SetActive(false);
            Debug.Log("Inventory full panel hidden");
        }
    }
    
    public bool HasEmptySlot()
    {
        bool empty = FindEmptySlot() != null;
        Debug.Log($"HasEmptySlot check: {empty}");
        return empty;
    }
    
    private void OnDestroy()
    {
        // Clean up
        CancelInvoke(nameof(HideInventoryFullPanel));
        
        if (inventoryFullCloseButton != null)
            inventoryFullCloseButton.onClick.RemoveAllListeners();
    }
}
