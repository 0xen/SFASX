using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotController : MonoBehaviour
{
    // The image that makes up the item slot
    public Image itemImage;
    // Item count text
    public TextMeshProUGUI text;
    // What color the box should be when selected 
    public Color selectedColor;
    // What color the box should be when deselected 
    public Color deselectedColor;
    // What color the text should be when selected 
    public Color selectedTextColor;
    // What color the text should be when deselected 
    public Color deselectedTextColor;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Add a item to the item slot
    public void AddItem(Sprite sprite, uint amount)
    {
        text.text = amount.ToString();
        itemImage.sprite = sprite;
        itemImage.enabled = true;
    }

    // Set if the slot is selected or not
    public void SetSelectedState(bool state)
    {
        GetComponent<Image>().color = (state ? selectedColor : deselectedColor);
        text.color = (state ? selectedTextColor : deselectedTextColor);
    }

    // Clear a item from the item slot
    public void RemoveItem()
    {
        text.text = "";
        itemImage.enabled = false;
    }

}
