using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using static WorldGenerator;

public class Game : MonoBehaviour
{
    public static GenerationPayload MapGenerationPayload = null;



    [SerializeField] private Camera MainCamera = null;
    [SerializeField] private Character Character = null;
    [SerializeField] private UIActionSelector ActionSelector = null;
    [SerializeField] private CameraController CameraController = null;
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

    /*
    [SerializeField]
    public struct DayColor
    {
        public float timeLength;
        public float brightness;
        public Color color;
    }

    [SerializeField] private DayColor[] DaylightScheduler;*/


    private EnvironmentTile mCurrentHoveredTile = null;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Environment mMap;
    private int mInterfaceState;

    // Time how long between the mouse has been held and released to decide if a quick action will be used or a menu select
    private float mMouseHoldTime;

    private readonly int NumberOfRaycastHits = 5;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        CameraController.Character = mCharacter;
        mMouseHoldTime = 0.0f;
        mInterfaceState = 0;
        ActionSelector.SetEnabled(false);
        Generate();
        CameraController.SetFollowing(true);
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


        Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
        // See what tiles are in the way of the cursor
        int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
        if (hits > 0)
        {
            // Calculate the closest tile from the cursor
            RaycastHit closestHit = mRaycastHits[0];
            float distance = (Camera.main.transform.position - closestHit.transform.position).magnitude;
            for(int i= 1; i < hits; i++)
            {
                float currentDistance = (Camera.main.transform.position - mRaycastHits[i].transform.position).magnitude;
                if(currentDistance < distance)
                {
                    distance = currentDistance;
                    closestHit = mRaycastHits[i];
                }
            }


            EnvironmentTile tile = closestHit.transform.GetComponent<EnvironmentTile>();
            // If we have not already selected the tile, change its tint
            if (tile != mCurrentHoveredTile)
            {
                tile.SetTint(new Color(1.0f, 0.75f, 0.75f));

                // If we have had a previous selected tile, then reset its tint
                if (mCurrentHoveredTile != null)
                {
                    mCurrentHoveredTile.SetTint(Color.white);
                }

                mCurrentHoveredTile = tile;

            }
        }
        else
        {
            // If we are not hitting any tiles, reset the current tiles tint
            if(mCurrentHoveredTile!=null)
            {
                mCurrentHoveredTile.SetTint(Color.white);
                mCurrentHoveredTile = null;
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            // Find out what actions are available from the current location and store them
            GatherActionList();
        }
        if (Input.GetMouseButtonUp(0))
        {
            ActionSelector.Select(mCharacter);
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

    void GatherActionList()
    {
        ActionSelector.actions.Clear();
        if (mCurrentHoveredTile!=null)
        {
            EnvironmentTile tile = mCurrentHoveredTile.transform.GetComponent<EnvironmentTile>();


            foreach(Material m in tile.GetComponent<MeshRenderer>().materials)
            {
                m.SetColor("_Tint", new Color(0.8f, 0.8f, 0.8f));
            }

            if (tile != null)
            {
                foreach (var component in tile.GetComponents<TileAction>())
                {
                    ActionSelector.actions.Add(component);
                }
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

    public void Generate()
    {
        if(MapGenerationPayload!=null)
        {
            mMap.GenerateWorld(mCharacter, MapGenerationPayload);
        }
    }

}
