using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public enum InventoryChangeEvent
    {
        Add,
        Remove
    }


    public EnvironmentTile CurrentPosition { get; set; }
    public int InventorySize { get; }

    public string entityName = "";

    public Item[] inventory;

    public float movmentSpeed;

    protected TileAction mAction;

    public Entity(int inventorySize)
    {
        InventorySize = inventorySize;
        inventory = new Item[InventorySize];
        mAction = null;
    }

    public TileAction GetCurrentAction()
    {
        return mAction;
    }

    public virtual void SetCurrentAction(TileAction action)
    {
        mAction = action;
    }

    public virtual void ResetAction()
    {
        mAction = null;
    }

    public bool HasAction()
    {
        return mAction != null;
    }

    public float GetMovmentSpeed()
    {
        return movmentSpeed;
    }

    public bool HasItem(Item item, uint count = 1)
    {
        if (item == null) return false;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                continue;
            }
            if (inventory[i].itemName == item.itemName && inventory[i].count >= count) 
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
            if(inventory[i].itemName == item.itemName)
            {
                inventory[i].count += count;
                InventoryChange(item, count, InventoryChangeEvent.Add);
                return true;
            }
        }
        if (!hasEmptySpace) return false;

        item.count = count;
        inventory[emptySpaceID] = item;
        InventoryChange(item, count, InventoryChangeEvent.Add);
        return true;
    }

    public bool RemoveFromInventory(Item item, uint amount = 1)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i]!=null && inventory[i].itemName == item.itemName && inventory[i].count >= amount)
            {
                inventory[i].count-= amount;
                if (inventory[i].count == 0)
                    inventory[i] = null;
                InventoryChange(item, amount, InventoryChangeEvent.Remove);
                return true;
            }
        }

        return false;
    }

    public abstract Item GetHandItem();

    public abstract void ChangeAnimation(AnimationStates state);

    public abstract void InventoryChange(Item item, uint count, InventoryChangeEvent eve);
}
