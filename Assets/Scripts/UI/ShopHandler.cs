using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopHandler : MonoBehaviour
{
    // Struct to store item, buy and sell price
    [System.Serializable]
    public struct ItemInstance
    {
        public uint buyPrice;
        public uint sellPrice;
        public Item item;
    }

    // Canvas that the shop UI is attached to
    [SerializeField] Canvas shopCanvas = null;

    // Items grid
    [SerializeField] GameObject ItemSelector = null;
    // Prefab used to show item image
    [SerializeField] ShopItem ShopItemPrefab = null;

    // Placeholder for category buttons
    [SerializeField] Button CatagoryButtonPlaceholder = null;
    // Parent to attach category buttons to
    [SerializeField] GameObject CatagoryButtonParent = null;


    // What is the title of the currently selected item
    [SerializeField] TextMeshProUGUI selectedItemTitle = null;
    // Description field for the items
    [SerializeField] TextMeshProUGUI selectedItemDescription = null;
    // Image for the selected item
    [SerializeField] Image selectedItemImage = null;


    // Bar that contains the buy and sell buttons
    [SerializeField] GameObject tradeBar = null;
    // Text used to show how many items you are buying / selling
    [SerializeField] TextMeshProUGUI itemAmountText = null;

    // Label for the players current currency
    [SerializeField] TextMeshProUGUI CurrentCoinText = null;

    // Players total currency
    private uint mCurrency = 0;

    // Amount of items that the player has selected to buy/sell
    private uint mAmount = 0;

    // What item is currently selected
    private ItemInstance selectedItem;


    [System.Serializable]
    public struct Catagory
    {
        public string name;
        public ItemInstance[] items;
    }
    // Category name and all items in the category
    [SerializeField] Catagory[] catagories = null;

    
    // Start is called before the first frame update
    void Start()
    {
        mAmount = 1;
        SetupCatagoryButtons();
        UpdateCoinText();
        AllCatagory();
    }

    // Get the current players currency
    public uint GetCurrency()
    {
        return mCurrency;
    }

    // Set the players current currency
    public void SetCurrency(uint currency)
    {
        mCurrency = currency;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Instantiate all the category buttons
    private void SetupCatagoryButtons()
    {
        //CatagoryButtonParent
        foreach (Catagory cat in catagories)
        {
            Button catButton = Instantiate(CatagoryButtonPlaceholder,new Vector3(0,0,0),Quaternion.identity, CatagoryButtonParent.transform);
            
            catButton.onClick.AddListener(delegate () {
                ClearCatagory();
                OpenCatagory(cat.name);
            });

            RectTransform recTransform = catButton.GetComponent<RectTransform>();
            recTransform.localScale = new Vector3(1, 1, 1);
            recTransform.localEulerAngles = new Vector3(0, 0, 0);
            recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);

            catButton.name = cat.name;

            // Access child text and change button text
            {
                if(recTransform.childCount>0)
                {
                    TextMeshProUGUI text = recTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
                    if(text!=null)
                    {
                        text.text = cat.name;
                    }
                }
            }
        }
    }

    // Buy the currently selected item
    public void Buy()
    {
        uint buyCost = mAmount * selectedItem.buyPrice;
        if(buyCost > mCurrency)
        {
            // Fail sound

            return;
        }
        mCurrency -= buyCost;
        UpdateCoinText();
        Environment.instance.GetCharacter().AddToInventory(selectedItem.item, mAmount);
    }

    // Sell from the players inventory
    public void Sell()
    {
        for(int i = 0; i < mAmount; i++)
        {
            if(!Environment.instance.GetCharacter().RemoveFromInventory(selectedItem.item))
            {
                break;
            }
            mCurrency += selectedItem.sellPrice;
        }
        UpdateCoinText();

    }

    // Update the ui text with the players current cash
    public void UpdateCoinText()
    {
        CurrentCoinText.text = mCurrency.ToString();
    }

    // Add 1 to the buy/sell
    public void IncreaseAmount()
    {
        if (mAmount < 99) mAmount++;

        itemAmountText.text = mAmount.ToString();
    }

    // Remove 1 from the buy/sell
    public void DecreaseAmount()
    {
        if (mAmount > 1) mAmount--;

        itemAmountText.text = mAmount.ToString();
    }

    // Open the items page
    public void OpenItem(ItemInstance item)
    {
        selectedItem = item;

        selectedItemTitle.text = item.item.itemName;

        selectedItemDescription.text = "Buy: " + item.buyPrice + " Sell:" + item.sellPrice + "\n\n" + item.item.description;

        selectedItemImage.sprite = item.item.itemSprite;
        selectedItemImage.color = Color.white;

        tradeBar.SetActive(true);
    }

    // Remove all items from the selection page
    public void ClearCatagory()
    {
        foreach (Transform child in ItemSelector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    // Open the all item page
    public void AllCatagory()
    {
        for(int i = 0; i < catagories.Length; i++)
        {
            OpenCatagory(catagories[i].items);
        }
    }

    // Open X category
    public void OpenCatagory(ItemInstance[] items)
    {
        // Loop through all items and add them to the UI
        for(int i = 0; i < items.Length; i++)
        {
            ShopItem item = GameObject.Instantiate(ShopItemPrefab);
            item.itemInstance = items[i];
            item.shopHandler = this;
            item.transform.SetParent(ItemSelector.transform);
            item.Start();

            RectTransform recTransform = item.GetComponent<RectTransform>();
            recTransform.localScale = new Vector3(1, 1, 1);
            recTransform.localEulerAngles = new Vector3(0, 0, 0);
            recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);
        } 
    }

    // Open X Category
    public void OpenCatagory(string name)
    {
        for (int i = 0; i < catagories.Length; i++)
        {
            if(catagories[i].name == name)
            {
                OpenCatagory(catagories[i].items);
                break;
            }
        }
    }

    // Toggle open or close the shop
    public void ToggleShop()
    {
        if(shopCanvas.enabled)
        {
            shopCanvas.enabled = false;
        }
        else
        {
            shopCanvas.enabled = true;
        }
    }
}
