using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("References")]
    [SerializeField] InventoryUI ui;
    [SerializeField] private Transform itemDropSpawnPoint; // Drag a child GameObject here as spawn point

    [Header("Prefabs")]
    [SerializeField] GameObject droppedItemPrefab;

    [Header("State")]
    [SerializeField] SerializedDictionary<string, ItemObject> inventory = new();

    public SerializedDictionary<string, ItemObject> Items => inventory;

    public bool HasItem(string itemId)
    {
        foreach (var item in inventory.Values)
        {
            if (item.id == itemId)
                return true;
        }
        return false;
    }

    public ItemObject GetItem(string itemId)
    {
        foreach (var item in inventory.Values)
        {
            if (item.id == itemId)
                return item;
        }
        return null;
    }

    public bool RemoveItemWithoutDrop(string itemId)
    {
        string itemToRemove = null;
        
        foreach (var kvp in inventory)
        {
            if (kvp.Value.id == itemId)
            {
                itemToRemove = kvp.Key;
                break;
            }
        }
        
        if (itemToRemove != null)
        {
            inventory.Remove(itemToRemove);
            if (ui != null)
                ui.RemoveUIItem(itemToRemove);
            return true;
        }
        
        return false;
    }

    private void Awake()
    {
        instance = this;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedItem"))
        {
            var droppedItem = other.GetComponent<DroppedItem>();
            if (droppedItem == null || droppedItem.pickedUp)
                return;
            
            // Check if inventory has empty slot
            if (ui != null && !ui.HasEmptySlot())
            {
                Debug.Log("Inventory is full! Can't pick up item.");
                
                // Show the inventory full panel
                ui.ShowInventoryFullPanel();
                return;
            }
            
            droppedItem.pickedUp = true;
            AddItem(droppedItem.item);
            Destroy(other.gameObject);

            if (AudioManager.instance != null)
                AudioManager.instance.Play("PickUp");
        }
    }

    void AddItem(ItemObject item)
    {
        if (item == null) return;
        
        var inventoryId = Guid.NewGuid().ToString();
        inventory.Add(inventoryId, item);
        
        // Try to add to UI - if fails (inventory full), remove from inventory
        if (ui != null && !ui.AddUIItem(inventoryId, item))
        {
            // Remove from inventory since we couldn't add to UI
            inventory.Remove(inventoryId);
            Debug.Log("Failed to add item to inventory UI");
            
            // Also show panel here as backup
            ui.ShowInventoryFullPanel();
        }
    }

    public void DropItem(string inventoryId)
    {
        if (inventory.TryGetValue(inventoryId, out ItemObject item))
        {
            // Use spawn point position if assigned, otherwise use player position
            Vector3 dropPosition = itemDropSpawnPoint != null ? 
                itemDropSpawnPoint.position : transform.position;
            
            // Create dropped item at calculated position
            if (droppedItemPrefab != null)
            {
                var droppedItem = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity).GetComponent<DroppedItem>();
                if (droppedItem != null)
                {
                    droppedItem.Initialize(item);
                }
            }
            
            // Remove from inventory
            inventory.Remove(inventoryId);
            
            // Remove from UI
            if (ui != null)
                ui.RemoveUIItem(inventoryId);
            
            // Play sound
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
            
            Debug.Log($"Dropped item at position: {dropPosition}");
        }
    }
}
