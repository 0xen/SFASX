using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileChange : MonoBehaviour
{

    public EnvironmentTile replacmentTile;

    public float minTime;
    public float maxTime;

    private float timeTillChange;
    private EnvironmentTile currentTile;

    // Start is called before the first frame update
    void Start()
    {
        timeTillChange = Random.Range(minTime, maxTime);
    }

    // Update is called once per frame
    void Update() 
    {
        timeTillChange -= Time.deltaTime;
        if(timeTillChange<0)
        {
            Environment.instance.ReplaceEnviromentTile(GetComponent<EnvironmentTile>(), replacmentTile);
        }
    }
}
