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
                InventoryChange();
            }
        }
    }

    public void SetUIItemBar(ItemSlotController[] uiItemBar)
    {
        mUiItemBar = uiItemBar;
        InventoryChange();
    }
    
    public override Item GetHandItem()
    {
        if (Inventory.Count < mSelectedItem || mSelectedItem < 0 || mSelectedItem > mUiItemBar.Length) return null;
        return Inventory[mSelectedItem];
    }
    public override void InventoryChange()
    {
        for(int i = 0; i < mUiItemBar.Length; i++)
        {
            if(i < Inventory.Count)
            {
                mUiItemBar[i].AddItem(Inventory[i].itemSprite, Inventory[i].count);
                mUiItemBar[i].SetSelectedState(mSelectedItem == i);
            }
            else
            {
                mUiItemBar[i].RemoveItem();
            }
        }


    }
}
