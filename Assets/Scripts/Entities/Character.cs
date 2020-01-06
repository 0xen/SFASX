using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : Entity
{
    const int CharacterInventorySize = 9 * 4;

    private int mSelectedItem;

    private ItemSlotController[] mUiItemBar;

    private List<TileAction> actionQue;

    public Character() : base(CharacterInventorySize)
    {
        mUiItemBar = null;
        mSelectedItem = -1;
        actionQue = new List<TileAction>();
    }

    public void AddActionToQue(TileAction action)
    {
        TileActionWalk walkAction = action as TileActionWalk;
        // If the new task is a walk task, calcel all previous tasks
        if (walkAction != null)
        {
            ClearActionQue();
            ResetAction();
        }
        actionQue.Add(action);
    }

    public void ClearActionQue()
    {
        actionQue.Clear();
    }

    private void Update()
    {
        if (!HasAction())
        {
            if (actionQue.Count > 0)
            {
                SetCurrentAction(actionQue[0]);
                actionQue.RemoveAt(0);
                GetCurrentAction().Run(this);
            }
        }

        if (mUiItemBar == null) return;

        for(int i = 0; i < mUiItemBar.Length; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (mSelectedItem == i)
                    mSelectedItem = -1;
                else
                    mSelectedItem = i;
                UpdateBar();
            }
        }
    }

    public void SetUIItemBar(ItemSlotController[] uiItemBar)
    {
        mUiItemBar = uiItemBar;
    }
    
    public override Item GetHandItem()
    {
        if (mSelectedItem<0) return null;
        return inventory[mSelectedItem];
    }

    private void UpdateBar()
    {
        if (inventory.Length <= 0) return;
        for (int i = 0; i < mUiItemBar.Length; i++)
        {
            if (inventory[i] != null)
            {
                mUiItemBar[i].AddItem(inventory[i].itemSprite, inventory[i].count);
            }
            else
            {
                mUiItemBar[i].RemoveItem();
            }
            mUiItemBar[i].SetSelectedState(mSelectedItem == i);
        }
    }

    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {
        if(eve== InventoryChangeEvent.Add)  Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
        UpdateBar();
    }
}
