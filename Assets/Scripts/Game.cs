using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera = null;
    [SerializeField] private Character Character = null;
    [SerializeField] private Canvas Menu = null;
    [SerializeField] private Canvas Hud = null;
    [SerializeField] private UIActionSelector ActionSelector = null;
    [SerializeField] private Transform CharacterStart = null;
    // How long a mouse button needs to be held before a click menu should open
    [SerializeField] private float MinMenuOpenTime = 0.6f;


    [SerializeField] private Light DirectionalLight = null;
    [SerializeField] private float DayLength = 120;
    private float mDayTime;
    [SerializeField] private float SunsetSunriseBrightness = 0.4f;
    [SerializeField] private float MidDayBrightness = 0.8f;
    [SerializeField] private Color MorningLightColor = Color.black;
    [SerializeField] private Color MidDayLightColor = Color.black;
    [SerializeField] private Color EveningLightColor = Color.black;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;
    private GameStates mGameState;
    private int mInterfaceState;

    // Time how long between the mouse has been held and released to decide if a quick action will be used or a menu select
    private float mMouseHoldTime;

    private readonly int NumberOfRaycastHits = 1;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        mMouseHoldTime = 0.0f;
        mInterfaceState = 0;
        ChangeState(GameStates.MainMenu);
    }

    private void Update()
    {
        // DayNight Cycle
        mDayTime += Time.deltaTime;
        if (mDayTime > DayLength) 
        {
            mDayTime = 0.0f;
        }

        {
            // Calculate the time of day in a range of 0-1
            float timeRed = mDayTime / DayLength;
            float lightDelta = Mathf.Lerp(0, 360.0f, timeRed);
            // Rotate the light from 0-360
            DirectionalLight.transform.eulerAngles = new Vector3(lightDelta, 90.0f, 0.0f);

            // If we are in the morning, lerp between morning -> Mid day color
            if (timeRed < 0.125f)
            {
                float lerp = timeRed / 0.125f;
                Color lightColor = Color.Lerp(MorningLightColor, MidDayLightColor, lerp);
                DirectionalLight.color = lightColor;
                Camera.main.backgroundColor = lightColor;
                DirectionalLight.intensity = Mathf.Lerp(SunsetSunriseBrightness, MidDayBrightness, lerp);
            }
            else // If we are going into evening, lerp between mid day and evening color
            if (timeRed > 0.35f)
            {
                float lerp = (timeRed - 0.35f) / 0.225f;
                Color lightColor = Color.Lerp(MidDayLightColor, EveningLightColor, lerp);
                DirectionalLight.color = lightColor;
                Camera.main.backgroundColor = lightColor;
                DirectionalLight.intensity = Mathf.Lerp(MidDayBrightness, SunsetSunriseBrightness, lerp);
            }



        }


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
                        ActionSelector.Select();
                        if (HasBit(mInterfaceState, (int)InterfaceState.ActionSelector))
                        {
                            ToggleState(InterfaceState.ActionSelector);
                        }
                        mMouseHoldTime = 0;
                    }

                    if (Input.GetMouseButton(0))
                    {
                        // If the button is being held, add to the mouse hold time
                        mMouseHoldTime += Time.deltaTime;
                        // If we have passed the time required for the popup menu and the flag for the menu being open is not set, open it
                        if (!HasBit(mInterfaceState, (int)InterfaceState.ActionSelector) && mMouseHoldTime > MinMenuOpenTime)
                        {
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
        ActionSelector.actions.Clear();

        Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
        int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
        if (hits > 0)
        {
            EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
            Debug.Log("Hit " + tile.name);
            if (tile != null)
            {
                // Temp
                ActionSelector.actions.Add(new TileActionWalk(tile, mMap, mCharacter));
                ActionSelector.actions.Add(new TileActionWalk(tile, mMap, mCharacter));
                ActionSelector.actions.Add(new TileActionWalk(tile, mMap, mCharacter));

                /*List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                mCharacter.GoTo(route);*/
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
                    ActionSelector.SetEnabled(!isActive);
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
                    ActionSelector.SetEnabled(false);
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
