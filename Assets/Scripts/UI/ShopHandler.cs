using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopHandler : MonoBehaviour
{

    [System.Serializable]
    public struct ItemInstance
    {
        public uint buyPrice;
        public uint sellPrice;
        public Item item;
    }

    [SerializeField] Canvas shopCanvas;

    [SerializeField] GameObject ItemSelector;
    [SerializeField] ShopItem ShopItemPrefab;

    [SerializeField] Button CatagoryButtonPlaceholder;
    [SerializeField] GameObject CatagoryButtonParent;


    [SerializeField] TextMeshProUGUI selectedItemTitle;
    [SerializeField] TextMeshProUGUI selectedItemDescription;
    [SerializeField] Image selectedItemImage;



    [SerializeField] GameObject tradeBar;
    [SerializeField] TextMeshProUGUI itemAmountText;


    [SerializeField] TextMeshProUGUI CurrentCoinText;

    private uint mCurrency;

    private uint mAmount;


    private ItemInstance selectedItem;


    [System.Serializable]
    public struct Catagory
    {
        public string name;
        public ItemInstance[] items;
    }
    [SerializeField] Catagory[] catagories;

    
    // Start is called before the first frame update
    void Start()
    {
        mAmount = 1;
        mCurrency = 500;
        SetupCatagoryButtons();
        UpdateCoinText();
        AllCatagory();
    }

    // Update is called once per frame
    void Update()
    {
    }

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

    public void Buy()
    {
        uint buyCost = mAmount * selectedItem.buyPrice;
        if(buyCost> mCurrency)
        {
            // Fail sound

            return;
        }
        mCurrency -= buyCost;
        UpdateCoinText();
        Environment.instance.GetCharacter().AddToInventory(selectedItem.item, mAmount);
    }

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

    public void UpdateCoinText()
    {
        CurrentCoinText.text = mCurrency.ToString();
    }

    public void IncreaseAmount()
    {
        if (mAmount < 99) mAmount++;

        itemAmountText.text = mAmount.ToString();
    }

    public void DecreaseAmount()
    {
        if (mAmount > 1) mAmount--;

        itemAmountText.text = mAmount.ToString();
    }

    public void OpenItem(ItemInstance item)
    {
        selectedItem = item;

        selectedItemTitle.text = item.item.itemName;

        selectedItemDescription.text = "Buy: " + item.buyPrice + " Sell:" + item.sellPrice;

        selectedItemImage.sprite = item.item.itemSprite;
        selectedItemImage.color = Color.white;

        tradeBar.SetActive(true);
    }

    public void ClearCatagory()
    {
        foreach (Transform child in ItemSelector.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void AllCatagory()
    {
        for(int i = 0; i < catagories.Length; i++)
        {
            OpenCatagory(catagories[i].items);
        }
    }

    public void OpenCatagory(ItemInstance[] items)
    {
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
