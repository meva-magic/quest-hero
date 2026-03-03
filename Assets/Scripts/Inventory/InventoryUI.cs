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
    private int originalSiblingIndex;
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
        
        // Находим слот под мышкой
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        if (raycaster != null)
            raycaster.Raycast(pointerData, results);
        
        InventorySlot targetSlot = null;
        
        // Сначала ищем прямой слот
        foreach (var result in results)
        {
            if (result.gameObject == null) continue;
            
            targetSlot = result.gameObject.GetComponent<InventorySlot>();
            if (targetSlot != null && slots.Contains(targetSlot))
            {
                break;
            }
            targetSlot = null;
        }
        
        // Если не нашли прямой слот, ищем через ItemUI
        if (targetSlot == null)
        {
            foreach (var result in results)
            {
                if (result.gameObject == null) continue;
                
                ItemUI itemUI = result.gameObject.GetComponent<ItemUI>();
                if (itemUI != null && itemUI.transform.parent != null)
                {
                    targetSlot = itemUI.transform.parent.GetComponent<InventorySlot>();
                    if (targetSlot != null && slots.Contains(targetSlot))
                    {
                        break;
                    }
                }
                targetSlot = null;
            }
        }
        
        // Если нашли целевой слот
        if (targetSlot != null)
        {
            Debug.Log($"Target slot: {targetSlot.name}, Child count: {targetSlot.transform.childCount}");
            
            // Целевой слот пустой
            if (targetSlot.transform.childCount == 0)
            {
                Debug.Log("Moving to empty slot");
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
                draggingObject.transform.localRotation = Quaternion.identity;
            }
            // В целевом слоте есть предмет
            else
            {
                Debug.Log("Swapping with occupied slot");
                Transform targetItem = targetSlot.transform.GetChild(0);
                
                // Сохраняем родителей для обмена
                Transform tempOriginalParent = originalParent;
                
                // Перемещаем перетаскиваемый предмет в целевой слот
                draggingObject.transform.SetParent(targetSlot.transform);
                draggingObject.transform.localPosition = Vector3.zero;
                draggingObject.transform.localRotation = Quaternion.identity;
                
                // Перемещаем целевой предмет в исходный слот
                targetItem.SetParent(tempOriginalParent);
                targetItem.localPosition = Vector3.zero;
                targetItem.localRotation = Quaternion.identity;
            }
        }
        else
        {
            Debug.Log("No slot found - returning to original");
            // Не нашли слот - возвращаем в исходный
            draggingObject.transform.SetParent(originalParent);
            draggingObject.transform.SetSiblingIndex(originalSiblingIndex);
            draggingObject.transform.localPosition = Vector3.zero;
            draggingObject.transform.localRotation = Quaternion.identity;
        }
        
        draggingItemId = null;
        draggingObject = null;
        originalParent = null;
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
