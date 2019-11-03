using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character Character;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Canvas ActionSelector;
    [SerializeField] private TextMeshProUGUI[] ActionSelectorLables;
    [SerializeField] private Transform CharacterStart;
    // How long a mouse button needs to be held before a click menu should open
    [SerializeField] private float MinMenuOpenTime;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;
    private GameStates mGameState;
    private int mInterfaceState;
    private int mButtonSelection;
    private List<TileActions> mTileActions;

    // Time how long between the mouse has been held and released to decide if a quick action will be used or a menu select
    private float mMouseHoldTime;

    private readonly int NumberOfRaycastHits = 1;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mTileActions = new List<TileActions>();
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        mMouseHoldTime = 0.0f;
        mInterfaceState = 0;
        mButtonSelection = -1;
        ChangeState(GameStates.MainMenu);
    }

    private void Update()
    {
        // Depending on what game state we are in, process its branch
        switch (mGameState)
        {
            case GameStates.MainMenu:
                {
                }
                break;
            case GameStates.InGame:
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        // Find out what actions are available from the current location and store them
                        GatherActionList();
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (mTileActions.Count == 0) break;
                        // If the mouse hold duration was too small, then we chose a quick selection from the action list
                        if (mMouseHoldTime < MinMenuOpenTime)
                        {
                            mTileActions[0].Run();
                        }
                        else
                        {
                            // If the button selection is valid, continue
                            if (mButtonSelection >= 0)
                                mTileActions[mButtonSelection].Run();
                        }
                        // Reset the action selector and the mouse variables
                        mMouseHoldTime = 0.0f;
                        mButtonSelection = -1;
                        if (ActionSelector.enabled)
                            ToggleState(InterfaceState.ActionSelector);
                    }

                    if (Input.GetMouseButton(0))
                    {
                        // If the button is being held, add to the mouse hold time
                        mMouseHoldTime += Time.deltaTime;
                        // If we have passed the time required for the popup menu and the flag for the menu being open is not set, open it
                        if (!HasBit(mInterfaceState, (int)InterfaceState.ActionSelector) && mMouseHoldTime > MinMenuOpenTime)
                        {
                            RenderActionLables();
                            ToggleState(InterfaceState.ActionSelector);
                        }
                    }
                }
                break;
        }
        

        /*if(Input.GetMouseButtonDown(0))
        {
        
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
            
        }*/
    }

    void GatherActionList()
    {
        mTileActions.Clear();

        Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
        int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
        if (hits > 0)
        {
            EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
            if (tile != null)
            {
                // Temp
                mTileActions.Add(new TileActionWalk(tile));
                mTileActions.Add(new TileActionWalk(tile));
                mTileActions.Add(new TileActionWalk(tile));
                mTileActions.Add(new TileActionWalk(tile));

                /*List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                mCharacter.GoTo(route);*/
            }







        }
    }

    void RenderActionLables()
    {
        for(int i = 0; i < ActionSelectorLables.Length; i ++)
        {
            if (mTileActions.Count <= i)
                ActionSelectorLables[i].gameObject.transform.parent.gameObject.SetActive(false);
            else
            {
                ActionSelectorLables[i].text = mTileActions[i].name;
                ActionSelectorLables[i].gameObject.transform.parent.gameObject.SetActive(true);
            }
        }
        
    }

    bool HasBit(int data, int bit)
    {
        return ((data >> bit) & 1) != 0;
    }

    void ClearBit(ref int data, int bit)
    {
        data &= ~(1 << bit);
    }

    void SetBit(ref int data, int bit)
    {
        data |= 0x1 << bit;
    }

    void ToggleBit(ref int data, int bit)
    {
        data ^= (1 << bit);
    }

    public void ActionSelection(int button)
    {
        mButtonSelection = button;
    }

    public void ToggleState(InterfaceState state)
    {
        // Dose the state flags already contain the state?
        bool isActive = HasBit(mInterfaceState,(int)state);

        ToggleBit(ref mInterfaceState, (int)state);

        switch (state)
        {
            case InterfaceState.ActionSelector:
                {
                    // Toggle the active/inactive state of the action selector
                    ActionSelector.enabled = !isActive;
                }
                break;
        }
    }

    public void OpenGame()
    {
        ChangeState(GameStates.InGame);
    }

    public void OpenMainMenu()
    {
        ChangeState(GameStates.MainMenu);
    }

    private void ChangeState(GameStates state)
    {
        mGameState = state;
        switch (state)
        {
            case GameStates.MainMenu: // ID 0
            case GameStates.InGame: // ID 1
                {
                    ActionSelector.enabled = false;
                    ShowMainMenu(state == GameStates.MainMenu);
                }
                break;
        }
    }

    public void ShowMainMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if( show )
            {
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                mMap.CleanUpWorld();
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;
            }
        }
    }

    public void Generate()
    {
        mMap.GenerateWorld();
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
