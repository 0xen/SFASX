using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileActions
{
    private Character mCharacter;

    public TileActionWalk(EnvironmentTile tile, Environment map, Character character) : base("Walk", tile, map)
    {
        mCharacter = character;
    }

    public override void Run()
    {
        Debug.Log("#Walking");
        List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, mTile);
        mCharacter.GoTo(route);
    }
}
