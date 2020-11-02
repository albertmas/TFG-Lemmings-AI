using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneManager : MonoBehaviour
{
    public Tilemap map;

    public List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    // Start is called before the first frame update
    void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var data in tileDatas)
        {
            foreach (var tile in data.tiles)
            {
                dataFromTiles.Add(tile, data);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get clicked tile
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPos = map.WorldToCell(mousePos);
            TileBase clickedTile = map.GetTile(gridPos);

            // Perform actions on selected tile depending on its type
            TileClicked(clickedTile, gridPos);
        }
    }

    void TileClicked(TileBase clickedTile, Vector3Int gridPos)
    {
        // Case empty tile
        if (clickedTile == null)
        {
            // Option to place umbrella if tile below is empty too
            // Option to build stairs if tile below is ground or continuing other stairs
            return;
        }

        bool result = dataFromTiles.TryGetValue(clickedTile, out TileData clickedTileData);
        if (result && clickedTileData.destructable)
            map.SetTile(gridPos, null);
    }

    public bool CheckForDamagingTile(Vector3 position)
    {
        // Get tile to check
        Vector3Int gridPos = map.WorldToCell(position);
        TileBase tileToCheck = map.GetTile(gridPos);

        if (tileToCheck != null)
        {
            // If tile is damaging return true
            bool result = dataFromTiles.TryGetValue(tileToCheck, out TileData clickedTileData);
            if (result && clickedTileData.damaging)
                return true;
        }

        return false;
    }
}
