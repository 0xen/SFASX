using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Character : Entity
{
    const int CharacterInventorySize = 9 * 4;

    private int mSelectedItem;

    private ItemSlotController[] mUiItemBar;
    private TextMeshProUGUI mUIItemMenuBarLable = null;
    
    private GameObject mUIActionSlotPrefab = null;
    private GameObject mUIActionBar = null;
    private int mUIMaxActionStream = 0;
    private TextMeshProUGUI mUIActionLable = null;

    private List<GameObject> mActiveActionSlots;


    private List<TileAction> actionQue;

    private Animator mAnimationController;

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

    public void AddActionToQue(TileAction action)
    {
        TileActionWalk walkAction = action as TileActionWalk;
        // If the new task is a walk task, calcel all previous tasks
        if (walkAction != null)
        {
            ClearActionQue();
            ResetAction();
        }
        actionQue.Add(action);
    }

    public void ClearActionQue()
    {
        foreach (TileAction action in actionQue)
        {
            action.environmentTile.SetTint(Color.white);
        }
        actionQue.Clear();
    }

    public override void SetCurrentAction(TileAction action)
    {
        if (mAction != null)
        {
            mAction.environmentTile.SetTint(Color.white);
        }
        base.SetCurrentAction(action);
    }

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

    public void SetUIItemBar(ItemSlotController[] uiItemBar, TextMeshProUGUI UIItemMenuBarLable)
    {
        mUiItemBar = uiItemBar;
        mUIItemMenuBarLable = UIItemMenuBarLable;
    }

    public void SetActionBar(GameObject UIActionSlotPrefab, GameObject UIActionBar, int UIMaxActionStream, TextMeshProUGUI UIActionLable)
    {
        mUIActionSlotPrefab = UIActionSlotPrefab;
        mUIActionBar = UIActionBar;
        mUIMaxActionStream = UIMaxActionStream;
        mUIActionLable = UIActionLable;
    }

    public override Item GetHandItem()
    {
        if (mSelectedItem<0) return null;
        return inventory[mSelectedItem];
    }

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

    public override void ChangeAnimation(AnimationStates state)
    {
        mAnimationController.SetInteger("Animation", (int)state);
    }

    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {
        if(eve== InventoryChangeEvent.Add)  Environment.instance.AddItemToPickupUI(item.itemName, count, item.itemSprite);
        UpdateBar();
    }
}
