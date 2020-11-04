using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneManager : MonoBehaviour
{
    public Tilemap map;

    public List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    public GameObject CellHighlight;

    Coroutine fadeSpriteCoroutine;


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
        // Highlight tile
        CellHighlight.SetActive(true);
        CellHighlight.transform.position = gridPos + map.cellSize / 2;
        CellHighlight.GetComponent<SpriteRenderer>().material.color = Color.white;

        // If highlight was fading, stop
        if (fadeSpriteCoroutine != null)
            StopCoroutine(fadeSpriteCoroutine);

        // Case empty tile
        if (clickedTile == null)
        {
            // Option to place umbrella if tile below is empty too
            // Option to build stairs if tile below is ground or continuing other stairs
            return;
        }

        bool result = dataFromTiles.TryGetValue(clickedTile, out TileData clickedTileData);
        if (result && clickedTileData.selectable)
        {
            if (clickedTileData.destructable)
                map.SetTile(gridPos, null);
        }
        else
        {
            // Fade cell highlight if cell cannot be selected
            fadeSpriteCoroutine = StartCoroutine(FadeSprite(CellHighlight.GetComponent<SpriteRenderer>(), 0f, 0.5f));
        }
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


    IEnumerator FadeSprite(SpriteRenderer sprite, float newAlpha, float fadeTime)
    {
        float alpha = sprite.material.color.a; // Save initial alpha
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / fadeTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, newAlpha, t));
            sprite.material.color = newColor;
            yield return null;
        }
    }
}
