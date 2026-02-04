using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("References")]
    [SerializeField] InventoryUI ui;

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
                Debug.Log("Inventory is full!");
                // Don't pick up, just reset the item's pickup state after a delay
                StartCoroutine(ResetPickupState(droppedItem));
                return;
            }
            
            droppedItem.pickedUp = true;
            AddItem(droppedItem.item);
            Destroy(other.gameObject);

            if (AudioManager.instance != null)
                AudioManager.instance.Play("PickUp");
        }
    }

    private System.Collections.IEnumerator ResetPickupState(DroppedItem droppedItem)
    {
        yield return new WaitForSeconds(0.5f);
        if (droppedItem != null)
            droppedItem.pickedUp = false;
    }

    void AddItem(ItemObject item)
    {
        if (item == null) return;
        
        var inventoryId = Guid.NewGuid().ToString();
        inventory.Add(inventoryId, item);
        
        // Try to add to UI - if fails (inventory full), remove from inventory
        if (ui != null && !ui.AddUIItem(inventoryId, item))
        {
            inventory.Remove(inventoryId);
            // Don't respawn - item wasn't picked up
        }
    }

    public void DropItem(string inventoryId)
    {
        if (inventory.TryGetValue(inventoryId, out ItemObject item))
        {
            var droppedItem = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity).GetComponent<DroppedItem>();
            if (droppedItem != null)
            {
                droppedItem.Initialize(item);
            }
            inventory.Remove(inventoryId);
            ui.RemoveUIItem(inventoryId);
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
        }
    }
}
