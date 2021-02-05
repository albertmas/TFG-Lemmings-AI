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

    [Tooltip("Is the tile part of the level structure?")]
    public bool structural;

    [Tooltip("Can the tile me demolished? Used for item tiles")]
    public bool demolishable;

    [Tooltip("Is this a top tile? Used for adding detail")]
    public bool top;

    [Tooltip("Is this a highlight tile? Used for marking placed items")]
    public bool highlight;

    [Tooltip("Is this a portal tile?")]
    public bool portal;
}
