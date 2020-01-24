using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{

    [System.Serializable]
    public enum TileType
    {
        Accessible,
        Inaccessible,
        Resource,
        Decorative
    }
    // Name of the tile
    public string TileName;
    // How is the tile connected to the other tiles
    public List<EnvironmentTile> Connections { get; set; }
    // When path finding, where did we connect from
    public EnvironmentTile Parent { get; set; }
    // What is the world space position of the tile
    public Vector3 Position { get; set; }
    // What is the grid space position of the tile
    public Vector2Int PositionTile { get; set; }
    // What is the current rotation of the tile
    public int Rotation { get; set; }
    // Global cost
    public float Global { get; set; }
    // Local cost
    public float Local { get; set; }
    // When path finding, have we been to this tile
    public bool Visited;
    // What type of tile
    public TileType Type;

    public void SetTint(Color color)
    {
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.SetColor("_Tint", color);
        }
    }
}
