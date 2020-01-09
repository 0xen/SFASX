using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static WorldGenerator;
using System.IO;

public class Game : MonoBehaviour
{
    public static GenerationPayload MapGenerationPayload = null;


    // Used for the loading and unloading of game items
    [SerializeField] private Item[] ItemInstances = null;

    [SerializeField] private Camera MainCamera = null;
    [SerializeField] private Character Character = null;
    [SerializeField] private UIActionSelector ActionSelector = null;
    [SerializeField] private CameraController CameraController = null;
    // How long a mouse button needs to be held before a click menu should open
    [SerializeField] private float MinMenuOpenTime = 0.6f;

    [SerializeField] private Light DirectionalLight = null;
    [SerializeField] private float DayLength = 120;
    [SerializeField] private float DayStartTime = 0;
    [SerializeField] private NotificationHandler NotificationHandler = null;
    [SerializeField] private TextMeshProUGUI TimeText = null;


    [SerializeField] private GameObject UiItemMenuBar = null;
    [SerializeField] private ItemSlotController UiItemMenuBarItem = null;
    [SerializeField] private uint UiItemMenuBarItemCount = 0;
    [SerializeField] private TextMeshProUGUI UIItemMenuBarLable = null;

    [SerializeField] private GameObject UIActionSlotPrefab = null;
    [SerializeField] private GameObject UIActionBar = null;
    [SerializeField] private int UIMaxActionStream = 0;
    [SerializeField] private TextMeshProUGUI UIActionLable = null;

    [SerializeField] private ShopHandler shop = null;

    private ItemSlotController[] mUiItemBar = null; 
    
    private float mDayTime = 0.0f;


    [System.Serializable]
    public struct DayColor
    {
        public float brightness;
        public Color color;
    }

    public DayColor[] DaylightScheduler;


    private EnvironmentTile mCurrentHoveredTile = null;

    private EnvironmentTile mCurrentAreaStart = null;
    private EnvironmentTile mCurrentAreaEnd = null;

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
        mUiItemBar = new ItemSlotController[UiItemMenuBarItemCount];

        for (int i = 0; i < UiItemMenuBarItemCount; i++)
        {
            mUiItemBar[i] = GameObject.Instantiate(UiItemMenuBarItem);
            mUiItemBar[i].transform.SetParent(UiItemMenuBar.transform);

            RectTransform recTransform = mUiItemBar[i].GetComponent<RectTransform>();
            recTransform.localScale = new Vector3(1, 1, 1);
            recTransform.localEulerAngles = new Vector3(0, 0, 0);
            recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);
        }

        mCharacter.SetUIItemBar(mUiItemBar, UIItemMenuBarLable);
        mCharacter.SetActionBar(UIActionSlotPrefab, UIActionBar, UIMaxActionStream, UIActionLable);
        CameraController.Character = mCharacter;
        mMouseHoldTime = 0.0f;
        mInterfaceState = 0;
        mDayTime = DayStartTime;
        ActionSelector.SetEnabled(false);
        ActionSelector.mCharacter = mCharacter;
        CameraController.SetFollowing(true);
        
        Generate();
        if(!MapGenerationPayload.loadFromFile)
            NotificationHandler.AddNotification(ref LandmarkNotification.NewGame, "Welcome to Celestia, throughout your time here, you will receive tips and tricks that will appear here!");
    }

    private void SetAreaColor(Vector2Int start, Vector2Int end, Color color)
    {
        int xMin = 0;
        int xMax = 0;
        int yMin = 0;
        int yMax = 0;

        if (start.x > end.x)
        {
            xMin = end.x;
            xMax = start.x;
        }
        else
        {
            xMin = start.x;
            xMax = end.x;
        }

        if (start.y > end.y)
        {
            yMin = end.y;
            yMax = start.y;
        }
        else
        {
            yMin = start.y;
            yMax = end.y;
        }


        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                EnvironmentTile tile = Environment.instance.GetTile(x, y);
                if (tile != null) tile.SetTint(color);
            }
        }
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
            // Change Time

            float timeFraction = mDayTime / DayLength;
            // Ofset the hours by 8 as the sun starts at a slightly rising angle
            int hour = (8 + (int)(24 * timeFraction)) % 24;
            // Get the hour in 12 hour format
            int hourTwelve = (hour % 12);
            // If we get a 0, we need to set it to 12
            if (hourTwelve == 0) hourTwelve = 12;
            // Get the total amount of 10 minutes in a day as we only need to change the tens part of the minute text
            int min = (int)((24 * 6) * timeFraction) % 6;
            TimeText.text = hourTwelve + ":" + min + "0 " + (hour > 11 ? "pm" : "am");



            // Calculate the time of day in a range of 0-1
            float timeRed = mDayTime / DayLength;
            float lightDelta = Mathf.Lerp(0, 360.0f, timeRed);
            // Rotate the light from 0-360
            DirectionalLight.transform.eulerAngles = new Vector3(lightDelta, 90.0f, 0.0f);


            DayColor start;
            DayColor end;
            
            float dayColorFrackRange = 1.0f / DaylightScheduler.Length;

            int startIndex = Mathf.FloorToInt(mDayTime / (DayLength / DaylightScheduler.Length));


            start = DaylightScheduler[startIndex];
            end = DaylightScheduler[(startIndex + 1) % DaylightScheduler.Length];


            float temp = (timeRed - (dayColorFrackRange * startIndex)) / dayColorFrackRange;

            Color lightColor = Color.Lerp(start.color, end.color, temp);
            DirectionalLight.color = lightColor;
            Camera.main.backgroundColor = lightColor;
            DirectionalLight.intensity = Mathf.Lerp(start.brightness, end.brightness, temp);


        }

        Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
        // See what tiles are in the way of the cursor
        int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
        if (hits > 0 && !EventSystem.current.IsPointerOverGameObject())
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
            if (tile!=null && tile != mCurrentHoveredTile)
            {
                tile.SetTint(new Color(1.0f, 0.75f, 0.75f));

                // If we have had a previous selected tile, then reset its tint
                if (mCurrentHoveredTile != null)
                {
                    mCurrentHoveredTile.SetTint(Color.white);
                }

                if (mCurrentAreaStart != null && mCurrentHoveredTile!=null)
                {
                    SetAreaColor(mCurrentAreaStart.PositionTile, mCurrentHoveredTile.PositionTile, Color.white);

                    SetAreaColor(mCurrentAreaStart.PositionTile, tile.PositionTile, Color.cyan);

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
            // Process Area Click
            if(mCurrentAreaStart!=null && mCurrentAreaEnd!=null)
            {
                ActionSelector.Select(mCurrentAreaStart.PositionTile, mCurrentAreaEnd.PositionTile);
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    mCurrentAreaStart = mCurrentHoveredTile;
                }
                // Find out what actions are available from the current location and store them
                GatherActionList();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if(mCurrentAreaEnd!=null)
            {
                // We are closing the selector from the area select
                ToggleState(InterfaceState.ActionSelector);
                mCurrentAreaStart = null;
                mCurrentAreaEnd = null;
            }
            else if (mCurrentAreaStart!=null)
            {
                SetAreaColor(mCurrentAreaStart.PositionTile, mCurrentHoveredTile.PositionTile, Color.white);
                mCurrentAreaEnd = mCurrentHoveredTile;
                mCurrentHoveredTile = null;
                ToggleState(InterfaceState.ActionSelector);
            }
            else
            {
                ActionSelector.Select();
                if (HasBit(mInterfaceState, (int)InterfaceState.ActionSelector))
                {
                    ToggleState(InterfaceState.ActionSelector);
                }
            }
            mMouseHoldTime = 0;
        }

        if (Input.GetMouseButton(0))
        {
            // Are we not in area select mode
            if(mCurrentAreaStart==null)
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
    }

    private void AttachTileActionsToSelector(TileAction[] actions, EnvironmentTile tile)
    {
        foreach (var action in actions)
        {
            action.environmentTile = tile;
            if (action.Valid(mCharacter))
            {
                ActionSelector.actions.Add(action);
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

                Entity[] entitiesOnTile = Environment.instance.GetEntitiesAt(tile.PositionTile);

                foreach (Entity e in entitiesOnTile)
                {
                    AttachTileActionsToSelector(e.GetComponents<TileAction>(), tile);
                }

                AttachTileActionsToSelector(tile.GetComponents<TileAction>(), tile);

                Item item = mCharacter.GetHandItem();
                if (item != null)
                {
                    foreach (var action in item.GetComponents<TileAction>())
                    {
                        action.environmentTile = tile;
                        if (action.Valid(mCharacter))
                        {
                            // We must instantiate a instance of the current action.
                            // The reason for this is a edge case when you have a item action that has reference to a tile,
                            // then you select the option multiple times. Only the last one will be recognized and the
                            // tile highlighting will go crazy as it is pointing to unknown tiles
                            TileAction actionInstance = TileAction.Instantiate(action);
                            actionInstance.environmentTile = tile;
                            ActionSelector.actions.Add(actionInstance);
                        }
                    }
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

    [System.Serializable]
    public struct EntitySaveData
    {
        // Name (Written as 'n' as it will reduce JSON file size)
        public string N;
        public int X;
        public int Y;
    }

    [System.Serializable]
    public struct ItemSaveData
    {
        // Name (Written as 'n' as it will reduce JSON file size)
        public string N;
        // Count (Written as 'c' as it will reduce JSON file size)
        public uint C;
    }

    [System.Serializable]
    public struct TileSaveData
    {
        public TileSaveData(string name, int rotation) { N = name; R = rotation; }
        // Name (Written as 'n' as it will reduce JSON file size)
        public string N;
        // Rotation (Written as 'r' as it will reduce JSON file size)
        public int R;
    }

    // This will contain the save data required when loading/unloading the game
    public struct SaveDataPacket
    {
        // Player Data
        public uint Money;
        public int PlayerX;
        public int PlayerY;
        public float Time;
        public ItemSaveData[] Inventory;

        // Entity Data
        public EntitySaveData[] Entities;

        // World Data
        public int WorldWidth;
        public int WorldHeight;
        public bool[] WaterMap;
        // All non water tiles
        public TileSaveData[] TileData;
    }

    public SaveDataPacket Load()
    {
        SaveDataPacket packet = JsonUtility.FromJson<SaveDataPacket>(File.ReadAllText("GameSave.json"));

        Environment environment = mMap;

        MapGenerationPayload.size.x = packet.WorldWidth;
        MapGenerationPayload.size.y = packet.WorldHeight;


        return packet;
    }

    public void Save()
    {
        Environment environment = mMap;

        SaveDataPacket packet = new SaveDataPacket();


        // Player Data
        packet.Money = shop.GetCurrency();
        packet.Time = mDayTime;
        packet.PlayerX = mCharacter.CurrentPosition.PositionTile.x;
        packet.PlayerY = mCharacter.CurrentPosition.PositionTile.y;

        packet.Inventory = new ItemSaveData[mCharacter.inventory.Length];
        for (int i = 0; i < mCharacter.inventory.Length; i++)
        {
            if (mCharacter.inventory[i] != null)
            {
                packet.Inventory[i].N = mCharacter.inventory[i].itemName;
                packet.Inventory[i].C = mCharacter.inventory[i].count;
            }
        }




        // Entity Data
        Entity[] entities = environment.GetEntities();
        packet.Entities = new EntitySaveData[entities.Length];
        for (int i = 0; i < entities.Length; i++)
        {
            packet.Entities[i].N = entities[i].entityName;
            packet.Entities[i].X = entities[i].CurrentPosition.PositionTile.x;
            packet.Entities[i].Y = entities[i].CurrentPosition.PositionTile.y;
        }


        // World Data
        environment.Save(ref packet);




        string jsonData = JsonUtility.ToJson(packet, false);
        File.WriteAllText("GameSave.json", jsonData);
    }

    public void Generate()
    {
        if (MapGenerationPayload != null)
        {
            if(MapGenerationPayload.loadFromFile)
            {
                SaveDataPacket saveGame = Load();
                // Load from file
                mMap.GenerateWorld(mCharacter, MapGenerationPayload, saveGame);

                // Load players inventory
                {
                    for(int i = 0; i < saveGame.Inventory.Length;i++)
                    {
                        foreach(Item item in ItemInstances)
                        {
                            if (item.itemName == saveGame.Inventory[i].N)
                            {
                                mCharacter.inventory[i] = Instantiate(item);
                                mCharacter.inventory[i].count = saveGame.Inventory[i].C;
                            }
                        }
                    }
                    mCharacter.UpdateBar();
                }

                // Load players money
                {
                    shop.SetCurrency(saveGame.Money);
                }

                // Load Time info
                {
                    mDayTime = saveGame.Time;
                }
            }
            else
            {
                // New Game
                mMap.GenerateWorld(mCharacter, MapGenerationPayload);
                shop.SetCurrency(500);
            }
        }
    }
}
