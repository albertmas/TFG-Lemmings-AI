using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;

    [Tooltip("Can the tile be selected?")]
    public bool selectable;

    [Tooltip("Can the tile be mined?")]
    public bool destructable;

    [Tooltip("Can the tile damage a creature?")]
    public bool damaging;

    [Tooltip("Can the tile me demolished? Used for item tiles")]
    public bool demolishable;
}
