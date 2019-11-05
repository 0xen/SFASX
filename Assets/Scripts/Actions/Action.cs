using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public string name;

    public Action(string _name)
    {
        name = _name;
    }

    public abstract void Run();
}
