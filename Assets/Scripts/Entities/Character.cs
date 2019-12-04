using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : Entity
{
    const int CharacterInventorySize = 9 * 4;

    private ItemSlotController[] mUiItemBar;

    public Character() : base(CharacterInventorySize)
    {

    }

    public void SetUIItemBar(ItemSlotController[] uiItemBar)
    {
        mUiItemBar = uiItemBar;
        InventoryChange();
    }

    public override void InventoryChange()
    {
        for(int i = 0; i < mUiItemBar.Length; i++)
        {
            if(i < Inventory.Count)
            {
                mUiItemBar[i].AddItem(Inventory[i].itemSprite, Inventory[i].count);
            }
            else
            {
                mUiItemBar[i].RemoveItem();
            }
        }


    }
}
