using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotController : MonoBehaviour
{

    public Image itemImage;
    public TextMeshProUGUI text;

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

    public void RemoveItem()
    {
        text.text = "";
        itemImage.enabled = false;
    }

}
