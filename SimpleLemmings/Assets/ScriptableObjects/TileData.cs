using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;

    [Tooltip("Can the tile be mined?")]
    public bool destructable;

    [Tooltip("Can the tile damage a creature?")]
    public bool damaging;
}
