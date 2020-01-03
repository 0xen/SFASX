using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{

    public EnvironmentTile CurrentPosition { get; set; }
    public int InventorySize { get; }

    public Item[] inventory;

    public float movmentSpeed;

    public Entity(int inventorySize)
    {
        InventorySize = inventorySize;
        inventory = new Item[InventorySize];
    }

    public float GetMovmentSpeed()
    {
        return movmentSpeed;
    }

    public bool HasItem(Item item, uint count = 1)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                continue;
            }
            if (inventory[i] == item && inventory[i].count >= count) 
            {
                return true;
            }
        }
        return false;
    }

    // Add a item to the entities inventory, if there is no space, a failed bool will be returned
    public bool AddToInventory(Item item, uint count)
    {
        bool hasEmptySpace = false;
        int emptySpaceID = -1;
        for (int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i]==null)
            {
                if(!hasEmptySpace)
                    emptySpaceID = i;
                hasEmptySpace = true;
                continue;
            }
            if(inventory[i] == item)
            {
                inventory[i].count += count;
                InventoryChange();
                Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
                return true;
            }
        }
        if (!hasEmptySpace) return false;

        item.count = count;
        inventory[emptySpaceID] = item;
        InventoryChange();
        Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
        return true;
    }

    public bool RemoveFromInventory(Item item, uint amount = 1)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == item && inventory[i].count >= amount)
            {
                inventory[i].count-= amount;
                if (inventory[i].count == 0)
                    inventory[i] = null;
                InventoryChange();
                return true;
            }
        }

        return false;
    }

    public abstract Item GetHandItem();

    public abstract void InventoryChange();
}
