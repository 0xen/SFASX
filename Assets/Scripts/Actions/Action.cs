using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public string tileName;

    public Action(string _tileName)
    {
        tileName = _tileName;
    }

    public abstract void Run();
}
