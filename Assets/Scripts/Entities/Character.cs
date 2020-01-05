using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : Entity
{
    const int CharacterInventorySize = 9 * 4;

    private int mSelectedItem;

    private ItemSlotController[] mUiItemBar;

    public Character() : base(CharacterInventorySize)
    {
        mUiItemBar = null;
        mSelectedItem = -1;
    }

    private void Update()
    {
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
