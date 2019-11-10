using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{

    public EnvironmentTile CurrentPosition { get; set; }
    public int InventorySize { get; }

    public List<Item> Inventory;

    public Entity(int inventorySize)
    {
        InventorySize = inventorySize;
        Inventory = new List<Item>();
    }

    // Add a item to the entities inventory, if there is no space, a failed bool will be returned
    public bool AddToInventory(Item item)
    {
        if (Inventory.Count >= InventorySize) return false;
        Inventory.Add(item);
        Debug.Log("Item added to inv");
        return true;
    }
    public bool RemoveFromInventory(Item item)
    {
        // To do
        return true;
    }
}
