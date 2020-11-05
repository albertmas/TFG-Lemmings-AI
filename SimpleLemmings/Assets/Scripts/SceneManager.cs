using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public Tilemap map;

    public List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    public GameObject CellHighlight;
    public GameObject UIBreakBlock;
    public GameObject UIPlaceUmbrella;

    public GameObject PrefabUmbrella;

    TileBase selectedTile;
    Vector3Int selectedTilePos;

    Coroutine fadeSpriteCoroutine;
    bool interactingWithCell = false;

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
        if (Input.GetMouseButton(0) && !interactingWithCell)
        {
            // Get selected tile
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPos = map.WorldToCell(mousePos);

            // Refresh highlighted tile
            if (gridPos != selectedTilePos)
            {
                selectedTile = map.GetTile(gridPos);
                selectedTilePos = gridPos;
                HighlightCell();
            }
        }

        if (Input.GetMouseButtonUp(0) && !interactingWithCell)
        {
            // Perform actions on the selected tile depending on its type
            TileClicked();
        }
        else if (Input.GetMouseButtonUp(0) && interactingWithCell)
        {
            interactingWithCell = false;
            CellHighlight.SetActive(false);
            UIBreakBlock.SetActive(false);
            UIPlaceUmbrella.SetActive(false);
        }
    }

    void HighlightCell()
    {
        // Highlight tile
        CellHighlight.SetActive(true);
        CellHighlight.transform.position = selectedTilePos + map.cellSize / 2;
        CellHighlight.GetComponent<SpriteRenderer>().material.color = Color.white;

        // If highlight was fading, stop
        if (fadeSpriteCoroutine != null)
        {
            StopCoroutine(fadeSpriteCoroutine);
            fadeSpriteCoroutine = null;
        }
    }

    void TileClicked()
    {
        // Case empty tile
        if (selectedTile == null)
        {
            // Option to place umbrella if tile below is empty too
            if (true)
            {
                Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
                worldPosition.y += map.cellSize.y * 1.5f; // Set position on top of the tile
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas
                UIPlaceUmbrella.SetActive(true);
                UIPlaceUmbrella.GetComponent<RectTransform>().position = buttonPos;

                interactingWithCell = true;
            }
            // Option to build stairs if tile below is ground or continuing other stairs
            // Option to place STOP if tile below is ground
            return;
        }

        bool result = dataFromTiles.TryGetValue(selectedTile, out TileData clickedTileData);
        if (result && clickedTileData.selectable)
        {
            if (clickedTileData.destructable)
            {
                Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
                worldPosition.y += map.cellSize.y * 1.5f; // Set position on top of the tile
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas
                UIBreakBlock.SetActive(true);
                UIBreakBlock.GetComponent<RectTransform>().position = buttonPos;

                interactingWithCell = true;
            }
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

    public void BreakBlock()
    {
        map.SetTile(selectedTilePos, null);
    }

    public void PlaceUmbrella()
    {
        Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        Instantiate(PrefabUmbrella, cellWorldPos, Quaternion.identity);
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
