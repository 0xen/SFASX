using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Character : Entity
{
    // How big is the players inventory
    const int CharacterInventorySize = 9;

    // What inventory item is currently selected
    private int mSelectedItem;

    // Ui Inventory slots
    private ItemSlotController[] mUiItemBar;
    // Label that shows the current items name
    private TextMeshProUGUI mUIItemMenuBarLable = null;
    
    // Prefab for the action icons
    private GameObject mUIActionSlotPrefab = null;
    // Parent for the action slot prefabs
    private GameObject mUIActionBar = null;
    // Max items that can be shown on the prefab
    private int mUIMaxActionStream = 0;
    // Label for showing what action is currently being used
    private TextMeshProUGUI mUIActionLable = null;

    // Action slots that are currently being displayed
    private List<GameObject> mActiveActionSlots;

    // Que of actions
    private List<TileAction> actionQue;

    // Characters animation controller
    private Animator mAnimationController;

    // Init the client and tell the Entity base class how big we want our inventory
    public Character() : base(CharacterInventorySize)
    {
        mUiItemBar = null;
        mSelectedItem = -1;
        actionQue = new List<TileAction>();
        mActiveActionSlots = new List<GameObject>();
    }

    public void Start()
    {
        mAnimationController = GetComponent<Animator>();
    }

    // Push a new action to the action que
    public void AddActionToQue(TileAction action)
    {
        TileActionWalk walkAction = action as TileActionWalk;
        // If the new task is a walk task, cancel all previous tasks
        if (walkAction != null)
        {
            ClearActionQue();
            ResetAction();
        }
        actionQue.Add(action);
    }

    // Remove all actions from the action que
    public void ClearActionQue()
    {
        foreach (TileAction action in actionQue)
        {
            action.environmentTile.SetTint(Color.white);
        }
        actionQue.Clear();
    }

    // Set the current action and clear the color of the current tile action
    public override void SetCurrentAction(TileAction action)
    {
        if (mAction != null)
        {
            mAction.environmentTile.SetTint(Color.white);
        }
        base.SetCurrentAction(action);
    }

    // Reset the current action
    public override void ResetAction()
    {
        if (mAction != null && mAction.environmentTile != null)
        {
            mAction.environmentTile.SetTint(Color.white);
        }
        base.ResetAction();
    }

    private void Update()
    {
        // Update Animator random animation seed
        mAnimationController.SetInteger("RandomSeed", Random.Range(0, 100));

        float timerCurrent = mAnimationController.GetFloat("Timer");
        timerCurrent += Time.deltaTime;
        if(timerCurrent>1.0f)
        {
            timerCurrent -= 1.0f;
            mAnimationController.SetBool("TimerTrigger", true);
        }
        else
        {
            mAnimationController.SetBool("TimerTrigger", false);
        }
        mAnimationController.SetFloat("Timer", timerCurrent);


        int activeActionSlots = actionQue.Count;
        // Add 1 to the action que to account for the current action being called
        if (mAction != null) activeActionSlots++;

        if (activeActionSlots > mUIMaxActionStream) activeActionSlots = mUIMaxActionStream;

        // Remove un-needed slots
        for (int i = mActiveActionSlots.Count-1; i>= activeActionSlots;i--)
        {
            Destroy(mActiveActionSlots[i]);
            mActiveActionSlots.RemoveAt(i);
        }
        // Add any new needed lots
        for (int i = mActiveActionSlots.Count; i < activeActionSlots; i++)
        {
            GameObject newSlot = GameObject.Instantiate(mUIActionSlotPrefab);
            newSlot.transform.SetParent(mUIActionBar.transform);
            RectTransform recTransform = newSlot.GetComponent<RectTransform>();
            recTransform.localScale = new Vector3(1, 1, 1);
            recTransform.localEulerAngles = new Vector3(0, 0, 0);
            recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);
            mActiveActionSlots.Add(newSlot);
        }

        // Is action slot 1 available
        if(mAction!=null)
        {
            mActiveActionSlots[0].GetComponent<Image>().sprite = mAction.actionImage;
        }

        // Set the textures of the action slots
        for (int i = 1; i < activeActionSlots; i++)
        {
            TileAction action = actionQue[i-1];
            mActiveActionSlots[i].GetComponent<Image>().sprite = action.actionImage;
        }



        for (int i = actionQue.Count - 1; i >= 0; i--)
        {
            if (actionQue[i] == null || actionQue[i].environmentTile == null)
            {
                actionQue.RemoveAt(i);
                continue;
            }
            actionQue[i].environmentTile.SetTint(new Color(0.0f, 0.75f, 0.75f));
        }

        if (!HasAction())
        {
            if (mUIActionLable != null) mUIActionLable.text = "";
            // Keep looping incase a action breaks stright out due to internal conditons not being met
            while (actionQue.Count > 0 && !HasAction())
            {
                SetCurrentAction(actionQue[0]);
                actionQue.RemoveAt(0);
                GetCurrentAction().Run(this);
            } 
        }
        else
        {
            if(mAction!=null && mAction.environmentTile!=null)
            {
                mAction.environmentTile.SetTint(new Color(0.0f, 0.75f, 0.1f));
                mUIActionLable.text = mAction.actionName;
            }
        }

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
    
    // Update the characters ui item bar
    public void SetUIItemBar(ItemSlotController[] uiItemBar, TextMeshProUGUI UIItemMenuBarLable)
    {
        mUiItemBar = uiItemBar;
        mUIItemMenuBarLable = UIItemMenuBarLable;
    }

    // Set the current action bar
    public void SetActionBar(GameObject UIActionSlotPrefab, GameObject UIActionBar, int UIMaxActionStream, TextMeshProUGUI UIActionLable)
    {
        mUIActionSlotPrefab = UIActionSlotPrefab;
        mUIActionBar = UIActionBar;
        mUIMaxActionStream = UIMaxActionStream;
        mUIActionLable = UIActionLable;
    }

    // Get the item in the players hand
    public override Item GetHandItem()
    {
        if (mSelectedItem<0) return null;
        return inventory[mSelectedItem];
    }

    // Update the item bar, set selected items, etc
    public void UpdateBar()
    {
        if (inventory.Length <= 0) return;
        for (int i = 0; i < mUiItemBar.Length; i++)
        {
            bool selected = mSelectedItem == i;
            if (inventory[i] != null)
            {
                if (selected)
                    mUIItemMenuBarLable.text = inventory[i].itemName;
                mUiItemBar[i].AddItem(inventory[i].itemSprite, inventory[i].count);
            }
            else
            {
                if (selected)
                    mUIItemMenuBarLable.text = "";
                mUiItemBar[i].RemoveItem();
            }
            mUiItemBar[i].SetSelectedState(selected);
        }
    }

    // Change the entities current animation
    public override void ChangeAnimation(AnimationStates state)
    {
        mAnimationController.SetInteger("Animation", (int)state);
    }

    // Called on a inventory change, count represents the amount of items inserted or removed
    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {
        if(eve== InventoryChangeEvent.Add)  Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
        UpdateBar();
    }
}
