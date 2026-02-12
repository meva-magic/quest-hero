using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [SerializeField] InventoryUI ui;
    [SerializeField] private Transform itemDropSpawnPoint;
    [SerializeField] GameObject droppedItemPrefab;
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
            
            if (ui != null && !ui.HasEmptySlot())
            {
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
        
        if (ui != null && !ui.AddUIItem(inventoryId, item))
        {
            inventory.Remove(inventoryId);
            ui.ShowInventoryFullPanel();
        }
    }

    public void DropItem(string inventoryId)
    {
        if (inventory.TryGetValue(inventoryId, out ItemObject item))
        {
            Vector3 dropPosition = itemDropSpawnPoint != null ? 
                itemDropSpawnPoint.position : transform.position;
            
            if (droppedItemPrefab != null)
            {
                var droppedItem = Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity).GetComponent<DroppedItem>();
                if (droppedItem != null)
                {
                    droppedItem.Initialize(item);
                }
            }
            
            inventory.Remove(inventoryId);
            
            if (ui != null)
                ui.RemoveUIItem(inventoryId);
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
        }
    }
}
