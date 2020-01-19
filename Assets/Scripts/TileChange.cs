using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileChange : MonoBehaviour
{
    // The tile that should be replaced after x time
    public EnvironmentTile replacmentTile;

    // Min and max time before the tile will change
    public float minTime;
    public float maxTime;

    // Chosen change time
    private float timeTillChange;

    // Start is called before the first frame update
    void Start()
    {
        timeTillChange = Random.Range(minTime, maxTime);
    }

    // Update is called once per frame
    void Update() 
    {
        // When the timeTillChange gets below 0, switch out the tile
        timeTillChange -= Time.deltaTime;
        if(timeTillChange<0)
        {
            Environment.instance.ReplaceEnviromentTile(GetComponent<EnvironmentTile>(), replacmentTile);
        }
    }
}
