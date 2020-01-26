using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static WorldGenerator;
using System.IO;
using UnityEngine.Audio;

public class Game : MonoBehaviour
{
    // Payload used to generate the environment
    public static GenerationPayload MapGenerationPayload = null;


    // Used for the loading and unloading of game items
    [SerializeField] private Item[] ItemInstances = null;
    // All items the player can start with
    [SerializeField] private Item[] PlayerStartingItems = null;

    // Main scene camera
    [SerializeField] private Camera MainCamera = null;
    // Character prefab
    [SerializeField] private Character Character = null;
    // Action selection menu
    [SerializeField] private UIActionSelector ActionSelector = null;
    // Camera controller
    [SerializeField] private CameraController CameraController = null;
    // How long a mouse button needs to be held before a click menu should open
    [SerializeField] private float MinMenuOpenTime = 0.6f;

    // Main directional light
    [SerializeField] private Light DirectionalLight = null;
    // Length of a in game day in seconds
    [SerializeField] private float DayLength = 120;
    // What time dose the new game start at
    [SerializeField] private float DayStartTime = 0;
    // Notification handler instance
    [SerializeField] private NotificationHandler NotificationHandler = null;
    // UI Text for the time
    [SerializeField] private TextMeshProUGUI TimeText = null;
    // Mini map Image
    [SerializeField] private RawImage MinimapImage = null;

    // Item UI menu bar
    [SerializeField] private GameObject UiItemMenuBar = null;
    // Prefab for UI menu bar items
    [SerializeField] private ItemSlotController UiItemMenuBarItem = null;
    // How many items can be seen on the ui menu bar
    [SerializeField] private uint UiItemMenuBarItemCount = 0;
    // Current selected item name on the ui
    [SerializeField] private TextMeshProUGUI UIItemMenuBarLable = null;

    // Prefab for the current action stream
    [SerializeField] private GameObject UIActionSlotPrefab = null;
    // Parent for current action items
    [SerializeField] private GameObject UIActionBar = null;
    // How many queued actions can we see at once
    [SerializeField] private int UIMaxActionStream = 0;
    // Label of what action is currently selected
    [SerializeField] private TextMeshProUGUI UIActionLable = null;
    // Instance of the shop
    [SerializeField] private ShopHandler shop = null;
    // UI slot controllers
    private ItemSlotController[] mUiItemBar = null;
    // Used to lerp between day/night time music
    [SerializeField] private AudioMixer MusicMixer = null;

    // Colors that are used on the list of text's that get dynamically colored
    [SerializeField] private Color dayTextColor;
    [SerializeField] private Color nightTextColor;
    // Text that should be colored based on day/night time
    [SerializeField] private List<TextMeshProUGUI> TextToDynamiclyColor = null;
     
    private float mDayTime = 0.0f;
    // Local map texture instance
    private Texture2D mMinimapVisulization = null;

    // Structure storing what color and brightness the light should be at a point of the day
    [System.Serializable]
    public struct DayColor
    {
        public float brightness;
        public Color color;
    }

    // The various points of the day, what light color/brightness should it be
    public DayColor[] DaylightScheduler;

    // What tile are we currently hovered over
    private EnvironmentTile mCurrentHoveredTile = null;

    // What is the start and end selection area for area selects
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

        // Add all item slot images
        for (int i = 0; i < UiItemMenuBarItemCount; i++)
        {
            mUiItemBar[i] = GameObject.Instantiate(UiItemMenuBarItem);
            mUiItemBar[i].transform.SetParent(UiItemMenuBar.transform);

            RectTransform recTransform = mUiItemBar[i].GetComponent<RectTransform>();
            recTransform.localScale = new Vector3(1, 1, 1);
            recTransform.localEulerAngles = new Vector3(0, 0, 0);
            recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);
        }
        // Init the character
        mCharacter.SetUIItemBar(mUiItemBar, UIItemMenuBarLable);
        mCharacter.SetActionBar(UIActionSlotPrefab, UIActionBar, UIMaxActionStream, UIActionLable);
        CameraController.Character = mCharacter;



        mMouseHoldTime = 0.0f;
        mInterfaceState = 0;
        mDayTime = DayStartTime;

        // Init the action selector
        ActionSelector.SetEnabled(false);
        ActionSelector.mCharacter = mCharacter;
        CameraController.SetFollowing(true);

        // Init the mini map
        mMinimapVisulization = new Texture2D(200, 200);
        MinimapImage.texture = mMinimapVisulization;
        
        Generate();
        if(!MapGenerationPayload.loadFromFile)
        {
            NotificationHandler.AddNotification(ref LandmarkNotification.NewGame,new[] {
                "Welcome to Celestia, throughout your time here, you will receive tips and tricks that will appear here!",
                "To select a tool, you can use numbers 1-9 on your keyboard. Try pressing 1 and cut down a tree by selecting one with the mouse left click",
                "You can queue up actions for your character by holding shift and clicking a action. Give it a try by selecting multiple trees.",
                "You can also preform a area select by pressing Left-CTRL and clicking on a object you want to interact with, then drag over the area and choose the action"
            }
            );
        }
    }

    // Update the minimap with the new player position
    void UpdateMinimap()
    {
        Vector2Int playerPosition = mCharacter.CurrentPosition.PositionTile;
        Vector2Int tempPosition = new Vector2Int();
        
        // Needs extracting out and made global
        int pixelSize = 7;

        int startX = playerPosition.x - ((mMinimapVisulization.width / 2) / pixelSize);
        int startY = playerPosition.y - ((mMinimapVisulization.height / 2) / pixelSize);

        // Loop through the length of the texture and increment in units of the "pixel" size
        for (int i = 0, tileX = startX; i < mMinimapVisulization.width; i+= pixelSize, tileX++)
        {

            // Loop for the length of a pixel on the x axis
            for (int x = i; x < i + pixelSize && x < mMinimapVisulization.width; x++)
            {

                // Loop through the length of the texture and increment in units of the "pixel" size
                for (int j = 0, tileY = startY; j < mMinimapVisulization.height; j+= pixelSize, tileY++)
                {
                    tempPosition.x = tileX;
                    tempPosition.y = tileY;

                    bool currentlyPlayerPosition = playerPosition == tempPosition;

                    // Loop for the length of a pixel on the y axis
                    for (int y = j; y < j + pixelSize && y < mMinimapVisulization.height; y++)
                    {
                        // If the tile coordinate is out of range or is not initialized, then clear the pixel
                        if (tileX < 0 || tileY < 0 || tileX >= Environment.instance.mMapGenerationPayload.size.x || tileY >= Environment.instance.mMapGenerationPayload.size.y
                            || Environment.instance.mMap[tileX][tileY] == null) // Dose the tile exist
                        {
                            mMinimapVisulization.SetPixel(x, y, Color.clear);
                        }
                        else if(currentlyPlayerPosition) // Draw the pixel color
                        {
                            mMinimapVisulization.SetPixel(x, y, Color.yellow);
                        }
                        else
                        {
                            EnvironmentTile tile = Environment.instance.mMap[tileX][tileY];
                            // Color the tiles based on what type of tile it is
                            switch (tile.Type)
                            {
                                case EnvironmentTile.TileType.Accessible:
                                    mMinimapVisulization.SetPixel(x, y, new Color(0.521f, 0.780f, 0.807f));
                                    break;
                                case EnvironmentTile.TileType.Resource:
                                    mMinimapVisulization.SetPixel(x, y, Color.gray);
                                    break;
                                case EnvironmentTile.TileType.Inaccessible:
                                    // Are we displaying a water tile or something else
                                    mMinimapVisulization.SetPixel(x, y, Environment.instance.mWaterMap[tileX, tileY] ? Color.cyan : new Color(0.305f, 0.627f, 0.592f));
                                    break;
                                case EnvironmentTile.TileType.Decorative:
                                    mMinimapVisulization.SetPixel(x, y, new Color(0.290f, 0.290f, 0.592f));
                                    break;
                            }
                        }
                    }
                }
            }
        }
        
        // Apply the new image texture
        mMinimapVisulization.Apply();

        MinimapImage.texture = mMinimapVisulization;
    }

    // Set a area selection color
    private void SetAreaColor(Vector2Int start, Vector2Int end, Color color)
    {
        int xMin = 0;
        int xMax = 0;
        int yMin = 0;
        int yMax = 0;

        // Calculate the min/max area of the selection
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

        // Loop through all the area and color it
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

        UpdateMinimap();

        // DayNight Cycle
        mDayTime += Time.deltaTime;
        if (mDayTime > DayLength) 
        {
            mDayTime = 0.0f;
        }

        {
            // Change Time

            // Calculate the time of day in a range of 0-1
            float timeFraction = mDayTime / DayLength;
            // Offset the hours by 8 as the sun starts at a slightly rising angle
            int hour = (8 + (int)(24 * timeFraction)) % 24;
            // Get the hour in 12 hour format
            int hourTwelve = (hour % 12);
            // If we get a 0, we need to set it to 12
            if (hourTwelve == 0) hourTwelve = 12;
            // Get the total amount of 10 minutes in a day as we only need to change the tens part of the minute text
            int min = (int)((24 * 6) * timeFraction) % 6;
            TimeText.text = hourTwelve + ":" + min + "0 " + (hour > 11 ? "pm" : "am");

            
            float lightDelta = Mathf.Lerp(0, 360.0f, timeFraction);
            // Rotate the light from 0-360
            DirectionalLight.transform.eulerAngles = new Vector3(lightDelta, 90.0f, 0.0f);


            DayColor start;
            DayColor end;
            
            float dayColorFrackRange = 1.0f / DaylightScheduler.Length;

            int startIndex = Mathf.FloorToInt(mDayTime / (DayLength / DaylightScheduler.Length));


            start = DaylightScheduler[startIndex];
            end = DaylightScheduler[(startIndex + 1) % DaylightScheduler.Length];


            float temp = (timeFraction - (dayColorFrackRange * startIndex)) / dayColorFrackRange;

            Color lightColor = Color.Lerp(start.color, end.color, temp);
            DirectionalLight.color = lightColor;
            Camera.main.backgroundColor = lightColor;
            DirectionalLight.intensity = Mathf.Lerp(start.brightness, end.brightness, temp);

            int dayTimeMusic = 6;
            int nightTimeMusic = 19;
            

            // Update audio based on time
            {
                float maxMusic = 0.0f;
                float minMusic = -80.0f;


                // If we are in day time hours
                if (hour > dayTimeMusic && hour < nightTimeMusic)
                {
                    MusicMixer.SetFloat("MusicVol", maxMusic);
                    MusicMixer.SetFloat("NightTimeMusicVol", minMusic);

                    foreach (TextMeshProUGUI text in TextToDynamiclyColor)
                        text.color = dayTextColor;
                }
                // Are we in night time hours
                else if(hour > nightTimeMusic || hour < dayTimeMusic)
                {
                    MusicMixer.SetFloat("MusicVol", minMusic);
                    MusicMixer.SetFloat("NightTimeMusicVol", maxMusic);

                    foreach (TextMeshProUGUI text in TextToDynamiclyColor)
                        text.color = nightTextColor;
                }
                else
                {
                    float musicLerpFactor = (1.0f / 60) * (((24 * 60) * timeFraction) % 60);
                    if (hour == dayTimeMusic) // Transitioning to day time
                    {
                        MusicMixer.SetFloat("MusicVol", Mathf.Lerp(minMusic, maxMusic, musicLerpFactor));
                        MusicMixer.SetFloat("NightTimeMusicVol", Mathf.Lerp(maxMusic, minMusic, musicLerpFactor));

                        foreach (TextMeshProUGUI text in TextToDynamiclyColor)
                        {
                            text.color = Color.Lerp(nightTextColor, dayTextColor, musicLerpFactor);
                        }
                    }
                    else if (hour == nightTimeMusic) // Transitioning to night time
                    {
                        MusicMixer.SetFloat("MusicVol", Mathf.Lerp(maxMusic, minMusic, musicLerpFactor));
                        MusicMixer.SetFloat("NightTimeMusicVol", Mathf.Lerp(minMusic, maxMusic, musicLerpFactor));

                        foreach (TextMeshProUGUI text in TextToDynamiclyColor)
                        {
                            text.color = Color.Lerp(dayTextColor, nightTextColor, musicLerpFactor);
                        }
                    }
                }
                
                
            }

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

    // Check to see if a bit is set
    bool HasBit(int data, int bit)
    {
        return ((data >> bit) & 1) != 0;
    }

    // Clear a bit in the data set
    void ClearBit(ref int data, int bit)
    {
        data &= ~(1 << bit);
    }

    // set a bit in the set
    void SetBit(ref int data, int bit)
    {
        data |= 0x1 << bit;
    }

    // Toggle a bit in the set
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

    // Load the game from a save file
    public SaveDataPacket Load()
    {
        SaveDataPacket packet = JsonUtility.FromJson<SaveDataPacket>(File.ReadAllText("GameSave.json"));

        Environment environment = mMap;

        MapGenerationPayload.size.x = packet.WorldWidth;
        MapGenerationPayload.size.y = packet.WorldHeight;
        
        return packet;
    }

    // Save the current game to a file
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
        
        // Save to the JSON file
        string jsonData = JsonUtility.ToJson(packet, false);
        File.WriteAllText("GameSave.json", jsonData);
    }

    // Generate the world
    // If the world is from a file, load it
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

                // Give the player there starting items
                foreach(Item i in PlayerStartingItems)
                {
                    mCharacter.AddToInventory(i, 1);
                }
            }
        }
    }
}
