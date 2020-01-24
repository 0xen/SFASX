using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    //  Item name
    public string itemName;
    // Item description
    public string description;
    // Amount of the item when in the inventory
    public uint count;
    // Sprite used to represent the item
    public Sprite itemSprite;
    // Dose the item get consumed on use
    public bool consumeOnUse;
}
