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

    private void Update()
    {

    }

    // Add a item to the entities inventory, if there is no space, a failed bool will be returned
    public bool AddToInventory(Item item, uint count)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            if(Inventory[i] == item)
            {
                Inventory[i].count += count;
                Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
                return true;
            }
        }
        if (Inventory.Count >= InventorySize) return false;
        item.count = count;
        Inventory.Add(item);
        Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
        Debug.Log("Item added to inv");
        return true;
    }

    public bool RemoveFromInventory(Item item)
    {
        // To do
        return true;
    }
    public Item GetHandItem()
    {
        if (Inventory.Count == 0) return null;
        return Inventory[0];
    }
}
