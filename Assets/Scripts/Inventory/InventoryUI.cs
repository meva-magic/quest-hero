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
    private CanvasGroup draggingCanvasGroup;
    private Transform originalParent;
    private int originalSiblingIndex;
    private bool isPanelShowing = false;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
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
        
        // Конвертируем позицию мыши в локальные координаты Canvas
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out mousePos
        );
        
        // Устанавливаем позицию прямо на курсор
        draggingRect.localPosition = mousePos;
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
        itemObj.transform.localRotation = Quaternion.identity;
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
        originalSiblingIndex = dragObject.transform.GetSiblingIndex();
        
        dragObject.transform.SetParent(canvas.transform);
        dragObject.transform.SetAsLastSibling();
        
        // Убеждаемся, что предмет видим
        dragObject.SetActive(true);
        
        // Настраиваем CanvasGroup для raycast
        CanvasGroup canvasGroup = dragObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = dragObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f; // Полная видимость
        draggingCanvasGroup = canvasGroup;
        
        UpdateDragPosition();
    }

    private InventorySlot GetSlotAtPosition(Vector2 screenPosition)
    {
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            if (slotRect == null) continue;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                slotRect, 
                screenPosition, 
                canvas.worldCamera, 
                out localPoint
            );
            
            if (slotRect.rect.Contains(localPoint))
            {
                return slot;
            }
        }
        
        return null;
    }

    private void EndDrag()
    {
        if (draggingItemId == null || draggingObject == null) return;
        
        if (draggingCanvasGroup != null)
        {
            draggingCanvasGroup.blocksRaycasts = true;
            draggingCanvasGroup = null;
        }
        
        InventorySlot targetSlot = GetSlotAtPosition(Input.mousePosition);
        
        if (targetSlot != null)
        {
            if (targetSlot.transform == originalParent)
            {
                ReturnToOriginalPosition();
            }
            else if (targetSlot.transform.childCount == 0)
            {
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
                draggingObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Transform targetItem = targetSlot.transform.GetChild(0);
                
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
                draggingObject.transform.localRotation = Quaternion.identity;
                
                targetItem.SetParent(originalParent);
                targetItem.localPosition = Vector3.zero;
                targetItem.localRotation = Quaternion.identity;
            }
        }
        else
        {
            ReturnToOriginalPosition();
        }
        
        draggingItemId = null;
        draggingObject = null;
        originalParent = null;
    }

    private void ReturnToOriginalPosition()
    {
        if (draggingObject != null && originalParent != null)
        {
            draggingObject.transform.SetParent(originalParent);
            draggingObject.transform.SetSiblingIndex(originalSiblingIndex);
            draggingObject.transform.localPosition = Vector3.zero;
            draggingObject.transform.localRotation = Quaternion.identity;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // EndDrag уже обрабатывает все
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
