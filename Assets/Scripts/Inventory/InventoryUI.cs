using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IDropHandler
{
    [SerializeField] GameObject uiItemPrefab;
    [SerializeField] Inventory inventory;
    [SerializeField] Transform uiInventoryParent;
    [SerializeField] private GameObject inventoryFullPanel;
    [SerializeField] private Button inventoryFullCloseButton;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 3;
    [SerializeField] SerializedDictionary<string, GameObject> inventoryUI = new();
    
    private string draggingItemId = null;
    private GameObject draggingObject = null;
    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private CanvasGroup draggingCanvasGroup;
    private Transform originalParent;
    private bool isPanelShowing = false;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        raycaster = GetComponentInParent<GraphicRaycaster>();
        
        InitializeSlots();
        InitializeInventoryFullPanel();
    }

    private void InitializeSlots()
    {
        if (slots.Count == 0 && uiInventoryParent != null)
        {
            var foundSlots = uiInventoryParent.GetComponentsInChildren<InventorySlot>();
            if (foundSlots != null)
            {
                slots.AddRange(foundSlots);
            }
        }
        
        if (slots.Count > maxSlots)
        {
            slots = slots.GetRange(0, maxSlots);
        }
    }

    private void InitializeInventoryFullPanel()
    {
        if (inventoryFullPanel == null) return;
        
        inventoryFullPanel.SetActive(false);
        isPanelShowing = false;
        
        if (inventoryFullCloseButton != null)
        {
            inventoryFullCloseButton.onClick.AddListener(() => 
            {
                if (inventoryFullPanel != null)
                {
                    inventoryFullPanel.SetActive(false);
                    isPanelShowing = false;
                }
            });
        }
    }

    private void Update()
    {
        if (draggingObject != null && Input.GetMouseButton(0))
        {
            UpdateDragPosition();
        }
        else if (draggingObject != null && !Input.GetMouseButton(0))
        {
            EndDrag();
        }
    }

    private void UpdateDragPosition()
    {
        if (draggingObject == null || canvas == null) return;
        
        RectTransform draggingRect = draggingObject.GetComponent<RectTransform>();
        if (draggingRect == null) return;
        
        Vector2 mousePos = Input.mousePosition;
        
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
        InventorySlot emptySlot = FindEmptySlot();
        
        if (emptySlot == null)
        {
            ShowInventoryFullPanel();
            return false;
        }
        
        if (uiItemPrefab == null || item == null) return false;
        
        GameObject itemObj = Instantiate(uiItemPrefab);
        ItemUI itemUI = itemObj.GetComponent<ItemUI>();
        
        if (itemUI == null)
        {
            Destroy(itemObj);
            return false;
        }
        
        itemObj.transform.SetParent(emptySlot.transform);
        itemObj.transform.localPosition = Vector3.zero;
        itemObj.transform.localScale = Vector3.one;
        
        inventoryUI.Add(inventoryId, itemObj);
        
        ItemDragHandler dragHandler = itemObj.AddComponent<ItemDragHandler>();
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
        originalParent = dragObject.transform.parent;
        
        dragObject.transform.SetAsLastSibling();
        
        draggingCanvasGroup = draggingObject.GetComponent<CanvasGroup>();
        if (draggingCanvasGroup == null)
            draggingCanvasGroup = draggingObject.AddComponent<CanvasGroup>();
        draggingCanvasGroup.blocksRaycasts = false;
        
        UpdateDragPosition();
    }

    private void EndDrag()
    {
        if (draggingItemId == null || draggingObject == null) return;
        
        if (draggingCanvasGroup != null)
        {
            draggingCanvasGroup.blocksRaycasts = true;
            draggingCanvasGroup = null;
        }
        
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
            if (targetSlot.transform.childCount == 0)
            {
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                SwapItems(draggingItemId, targetSlot);
            }
        }
        else
        {
            if (!IsPointerOverInventoryUI())
            {
                if (inventory != null)
                {
                    inventory.DropItem(draggingItemId);
                }
            }
            else
            {
                ReturnToOriginalPosition();
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
        if (inventoryFullPanel == null) return;
        
        if (isPanelShowing) return;
        
        inventoryFullPanel.SetActive(true);
        isPanelShowing = true;
        
        inventoryFullPanel.transform.SetAsLastSibling();
        
        if (canvas != null)
        {
            Canvas.ForceUpdateCanvases();
        }
        
        CancelInvoke(nameof(HideInventoryFullPanel));
        Invoke(nameof(HideInventoryFullPanel), 3f);
    }

    private void HideInventoryFullPanel()
    {
        if (inventoryFullPanel != null && inventoryFullPanel.activeSelf)
        {
            inventoryFullPanel.SetActive(false);
            isPanelShowing = false;
        }
    }
    
    public bool HasEmptySlot()
    {
        return FindEmptySlot() != null;
    }
    
    private void OnDestroy()
    {
        CancelInvoke(nameof(HideInventoryFullPanel));
        
        if (inventoryFullCloseButton != null)
            inventoryFullCloseButton.onClick.RemoveAllListeners();
    }
}
