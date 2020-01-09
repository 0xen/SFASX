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
    public string TileName;
    public List<EnvironmentTile> Connections { get; set; }
    public EnvironmentTile Parent { get; set; }
    public Vector3 Position { get; set; }
    public Vector2Int PositionTile { get; set; }
    public int Rotation { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited;// { get; set; }
    public TileType Type;

    public void SetTint(Color color)
    {
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.SetColor("_Tint", color);
        }
    }
}
