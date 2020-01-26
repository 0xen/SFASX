using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{

    // Are we adding or removing from the inventory
    public enum InventoryChangeEvent
    {
        Add,
        Remove
    }


    // Current position of the entity
    public EnvironmentTile CurrentPosition { get; set; }
    // Size of the entities inventory
    public int InventorySize { get; }
    // Name of the entity
    public string entityName = "";
    // Entities inventory
    public Item[] inventory;
    // How fast the entity can move
    public float movmentSpeed;
    // Current action that the entity is doing
    protected TileAction mAction;

    // Init the entity
    public Entity(int inventorySize)
    {
        InventorySize = inventorySize;
        inventory = new Item[InventorySize];
        mAction = null;
    }

    // Get the current action
    public TileAction GetCurrentAction()
    {
        return mAction;
    }

    // Set the entities current action
    public virtual void SetCurrentAction(TileAction action)
    {
        mAction = action;
    }

    // Reset the entities action
    public virtual void ResetAction()
    {
        mAction = null;
    }

    // Check to see if the entity has a action
    public bool HasAction()
    {
        return mAction != null;
    }

    // Get the entities movement speed
    public float GetMovmentSpeed()
    {
        return movmentSpeed;
    }

    // Check to see if the entity has a item
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
        for (int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i]==null)
            {
                item.count = count;
                inventory[i] = item;
                InventoryChange(item, count, InventoryChangeEvent.Add);
                return true;
            }
            if(inventory[i].itemName == item.itemName)
            {
                inventory[i].count += count;
                InventoryChange(item, count, InventoryChangeEvent.Add);
                return true;
            }
        }
        return false;
    }

    // Remove x of y item from the inventory
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

    // Get the item in the players hand
    public abstract Item GetHandItem();

    // Change the entities current animation
    public abstract void ChangeAnimation(AnimationStates state);

    // Called on a inventory change, count represents the amount of items inserted or removed
    public abstract void InventoryChange(Item item, uint count, InventoryChangeEvent eve);
}
