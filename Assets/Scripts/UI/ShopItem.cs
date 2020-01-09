using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    [SerializeField] Image prefabImage = null;
    public ShopHandler shopHandler = null;
    public ShopHandler.ItemInstance itemInstance;

    // Start is called before the first frame update
    public void Start()
    {
        prefabImage.sprite = itemInstance.item.itemSprite;
    }

    // Update is called once per frame
    public void Update()
    {
    }

    public void OpenItem()
    {
        shopHandler.OpenItem(itemInstance);
    }
}
