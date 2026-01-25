using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    private void Awake()
    {
        instance = this;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DroppedItem"))
        {
            var droppedItem = other.GetComponent<DroppedItem>();
            if (droppedItem.pickedUp)
            {
                return;
            }
            droppedItem.pickedUp = true;

            AddItem(droppedItem.item);
            Destroy(other.gameObject);

            AudioManager.instance.Play("PickUp");
        }
    }

    void AddItem(ItemObject item)
    {
        var inventoryId = Guid.NewGuid().ToString();
        inventory.Add(inventoryId, item);
        ui.AddUIItem(inventoryId, item);
    }

    public void DropItem(string inventoryId)
    {
        var droppedItem = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity).GetComponent<DroppedItem>();
        var item = inventory.GetValueOrDefault(inventoryId);
        droppedItem.Initialize(item);
        inventory.Remove(inventoryId);
        ui.RemoveUIItem(inventoryId);
        
        AudioManager.instance.Play("Drop");
    }
}
