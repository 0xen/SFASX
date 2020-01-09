using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotController : MonoBehaviour
{

    public Image itemImage;
    public TextMeshProUGUI text;
    public Color selectedColor;
    public Color deselectedColor;
    public Color selectedTextColor;
    public Color deselectedTextColor;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void AddItem(Sprite sprite, uint amount)
    {
        text.text = amount.ToString();
        itemImage.sprite = sprite;
        itemImage.enabled = true;
    }

    public void SetSelectedState(bool state)
    {
        GetComponent<Image>().color = (state ? selectedColor : deselectedColor);
        text.color = (state ? selectedTextColor : deselectedTextColor);
    }

    public void RemoveItem()
    {
        text.text = "";
        itemImage.enabled = false;
    }

}
