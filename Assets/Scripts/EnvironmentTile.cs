using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    public List<EnvironmentTile> Connections { get; set; }
    public EnvironmentTile Parent { get; set; }
    public Vector3 Position { get; set; }
    public Vector2Int PositionTile { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited { get; set; }
    public bool IsAccessible { get; set; }

    public void SetTint(Color color)
    {
        foreach (Material m in GetComponent<MeshRenderer>().materials)
        {
            m.SetColor("_Tint", color);
        }
    }
}
