using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Range(1, 5)]
    public int gameSpeed = 1;
    public bool playerInput = true;
    public bool agentPlaying = false;

    [Header("Tilemaps")]
    public Tilemap map;
    public Tilemap mapDetail;

    [Space]
    public List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    [Header("UI gameobjects")]
    public GameObject CellSelection;
    public GameObject UIBreakBlock;
    public GameObject UIPlaceUmbrella;
    public GameObject UIBuildRStairs;
    public GameObject UIBuildLStairs;
    public GameObject UIDemolishBlock;
    public GameObject UICross;
    public GameObject UIVictory;
    public GameObject UIDefeat;

    [Space]
    public GameObject PrefabUmbrella;
    public GameObject PrefabRStairs;
    public GameObject PrefabLStairs;

    [Header("Tiles")]
    public TileBase TileHighlight;
    public TileBase TileRStairs;
    public TileBase TileLStairs;
    public TileBase TileTop;

    [Space]
    public LemmingsAgent AIAgent;

    GameObject creature;
    SpawnCreatures spawner;

    public int MapWidth { get; private set; } = 16;
    public int MapHeight { get; private set; } = 8;

    TileBase selectedTile;
    [HideInInspector]
    public Vector3Int selectedTilePos = Vector3Int.one;

    Dictionary<Vector3Int, GameObject> placedUmbrellas;
    Dictionary<Vector3Int, GameObject> placedStairs;

    Coroutine fadeSpriteCoroutine;
    bool interactingWithCell = false;

    public int MaxActions { get; private set; } = 6;

    AudioSource audioSource;

    void Awake()
    {
        // Create Dictionary and fill it with each tile type and its data
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var data in tileDatas)
        {
            foreach (var tile in data.tiles)
            {
                dataFromTiles.Add(tile, data);
            }
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        placedUmbrellas = new Dictionary<Vector3Int, GameObject>();
        placedStairs = new Dictionary<Vector3Int, GameObject>();
        spawner = GameObject.FindGameObjectWithTag("Spawner").GetComponent<SpawnCreatures>();
        creature = spawner.SpawnCreature();
        Time.timeScale = gameSpeed;
    }

    void Update()
    {
        if (!playerInput) { return; }

        if (Input.GetKeyUp(KeyCode.R)) { RestartLevel(); }

        if (Input.GetMouseButton(0) && !interactingWithCell)
        {
            // Get newly selected tile
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPos = map.WorldToCell(mousePos);

            // Refresh selected tile
            if (gridPos != selectedTilePos)
            {
                selectedTile = map.GetTile(gridPos);
                selectedTilePos = gridPos;
                SelectCell();
            }
        }

        if (Input.GetMouseButtonUp(0) && !interactingWithCell)
        {
            // Perform actions on the selected tile depending on its type
            TileClicked();
        }
        else if (Input.GetMouseButtonUp(0) && interactingWithCell)
        {
            UnselectCell();
        }
    }

    void SelectCell()
    {
        // Select tile
        CellSelection.SetActive(true);
        CellSelection.transform.position = selectedTilePos + map.cellSize / 2;
        CellSelection.GetComponent<SpriteRenderer>().material.color = Color.white; // Reset alpha to 1f

        // If selection sprite was fading, stop
        if (fadeSpriteCoroutine != null)
        {
            StopCoroutine(fadeSpriteCoroutine);
            fadeSpriteCoroutine = null;
        }
    }

    void TileClicked()
    {
        if (!PlaceUIActions())
        {
            // Fade cell selection sprite if cell cannot be selected
            fadeSpriteCoroutine = StartCoroutine(FadeSprite(CellSelection.GetComponent<SpriteRenderer>(), 0f, 0.5f));
            selectedTilePos = Vector3Int.one; // Reset selected tile pos
        }
    }

    public int GetTileActions(Vector3Int tilePos, out bool[] availableActions)
    {
        availableActions = new bool[MaxActions];
        int numActions = 0;

        TileBase tile = map.GetTile(tilePos);
        if (tile != null)
        {
            bool result = dataFromTiles.TryGetValue(tile, out TileData clickedTileData);
            if (result && clickedTileData.destructable)
            {
                // 0 Break
                availableActions[0] = true;
                numActions++;
            }

            if (result && clickedTileData.demolishable)
            {
                // 4 Demolish Block
                availableActions[4] = true;
                numActions++;
            }

            if (result && clickedTileData.highlight)
            {
                // 5 Remove Umbrella
                availableActions[5] = true;
                numActions++;
            }
        }
        else
        {
            // If tile below is empty too (or has a non structural tile), an umbrella can be placed
            Vector3Int tileBelowPos = tilePos + Vector3Int.down;
            TileBase tileBelow = map.GetTile(tileBelowPos);
            if (tileBelow == null)
            {
                // 1 Umbrella
                availableActions[1] = true;
                numActions++;
            }
            else
            {
                bool result = dataFromTiles.TryGetValue(tileBelow, out TileData clickedTileData);
                if (result && !clickedTileData.structural)
                {
                    // 1 Umbrella
                    availableActions[1] = true;
                    numActions++;
                }
            }

            // Check if tile above is empty too (or has a non structural tile)
            bool noStructureAbove = false;
            TileBase tileAbove = map.GetTile(tilePos + Vector3Int.up);
            if (tileAbove == null) { noStructureAbove = true; }
            else
            {
                bool result = dataFromTiles.TryGetValue(tileAbove, out TileData clickedTileData);
                if (result && !clickedTileData.structural) { noStructureAbove = true; }
            }

            if (noStructureAbove)
            {
                // If there is a structural tile on the low left, a right stair can be placed
                TileBase tileLeft = map.GetTile(tilePos + Vector3Int.left + Vector3Int.down);
                if (tileLeft != null)
                {
                    bool result = dataFromTiles.TryGetValue(tileLeft, out TileData clickedTileData);
                    if (result && (clickedTileData.structural || clickedTileData.demolishable))
                    {
                        // 2 R Stairs
                        availableActions[2] = true;
                        numActions++;
                    }
                }

                // If there is a structural tile on the low right, a left stair can be placed
                TileBase tileRight = map.GetTile(tilePos + Vector3Int.right + Vector3Int.down);
                if (tileRight != null)
                {
                    bool result = dataFromTiles.TryGetValue(tileRight, out TileData clickedTileData);
                    if (result && (clickedTileData.structural || clickedTileData.demolishable))
                    {
                        // 3 L Stairs
                        availableActions[3] = true;
                        numActions++;
                    }
                }
            }
        }

        return numActions;
    }

    bool PlaceUIActions()
    {
        bool anyActionAvailable = false;

        int numActions = GetTileActions(selectedTilePos, out bool[] availableActions);

        if (numActions > 0)
        {
            anyActionAvailable = true;
            interactingWithCell = true;

            Vector3 worldPosition = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
            if (selectedTilePos.y < MapHeight - 2)
                worldPosition.y += map.cellSize.y * 1.5f; // Set position above the tile
            else
                worldPosition.y -= map.cellSize.y * 1.5f; // Set position below the tile if selecting a top tile

            if (numActions > 2 && selectedTilePos.x <= 1) { } // If cell is at the far left and has > 2 actions, don't move
            else if (numActions > 2 && selectedTilePos.x > MapWidth - 3) // If cell is at the far right and has > 2 actions
                worldPosition.x -= map.cellSize.x * 1.1f * (numActions - 1); // Move a full cell's width + offset for every action available minus one
            else
                worldPosition.x -= map.cellSize.x / 2 * 1.1f * (numActions - 1); // Move half a cell's width + offset for every action available minus one

            for (int i = 0; i < availableActions.Length; i++)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // Get position in screen coords
                RectTransformUtility.ScreenPointToWorldPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPosition,
                    null, out Vector3 buttonPos); // Transform screen coords to point in the canvas

                if (availableActions[i] == true)
                {
                    if (i == 0) // 0 Break
                    {
                        UIBreakBlock.SetActive(true);
                        UIBreakBlock.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                    else if (i == 1) // 1 Umbrella
                    {
                        UIPlaceUmbrella.SetActive(true);
                        UIPlaceUmbrella.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                    else if (i == 2) // 2 R Stairs
                    {
                        UIBuildRStairs.SetActive(true);
                        UIBuildRStairs.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                    else if (i == 3) // 3 L Stairs
                    {
                        UIBuildLStairs.SetActive(true);
                        UIBuildLStairs.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                    else if (i == 4) // 4 Demolish Stairs
                    {
                        UIDemolishBlock.SetActive(true);
                        UIDemolishBlock.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                    else if (i == 5) // 5 Remove Umbrella
                    {
                        UICross.SetActive(true);
                        UICross.GetComponent<RectTransform>().position = buttonPos;
                        worldPosition.x += map.cellSize.x * 1.1f; // Move a cell's width to the right
                    }
                }
            }
        }

        return anyActionAvailable;
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
        map.SetTile(selectedTilePos, null); // Remove this tile
        mapDetail.SetTile(selectedTilePos, null); // Romove detail tile on same pos

        // Check if detail tile above is a top tile. Remove it if so
        TileBase aboveTile = mapDetail.GetTile(selectedTilePos + Vector3Int.up);
        if (aboveTile)
        {
            bool result = dataFromTiles.TryGetValue(aboveTile, out TileData aboveTileData);

            if (result && aboveTileData.top)
                mapDetail.SetTile(selectedTilePos + Vector3Int.up, null);
        }

        // Check if there is a tile below. If so, add a top tile over it
        TileBase belowTile = map.GetTile(selectedTilePos + Vector3Int.down);
        if (belowTile)
        {
            bool result = dataFromTiles.TryGetValue(belowTile, out TileData belowTileData);

            if (result && belowTileData.structural)
                mapDetail.SetTile(selectedTilePos, TileTop);
        }

        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void PlaceUmbrella()
    {
        map.SetTile(selectedTilePos, TileHighlight);
        Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        GameObject umbrella = Instantiate(PrefabUmbrella, cellWorldPos, Quaternion.identity);
        placedUmbrellas.Add(selectedTilePos, umbrella);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void RemoveUmbrella()
    {
        map.SetTile(selectedTilePos, null); // Remove tile highlight
        // Remove umbrella
        placedUmbrellas.TryGetValue(selectedTilePos, out GameObject umbrella);
        if (umbrella)
        {
            Destroy(umbrella);
            placedUmbrellas.Remove(selectedTilePos);
        }
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void BuildRStairs()
    {
        Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        GameObject stairs = Instantiate(PrefabRStairs, cellWorldPos, Quaternion.identity);
        placedStairs.Add(selectedTilePos, stairs);
        map.SetTile(selectedTilePos, TileRStairs);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void BuildLStairs()
    {
        Vector3 cellWorldPos = map.CellToWorld(selectedTilePos) + map.cellSize / 2; // Get tile center position
        GameObject stairs = Instantiate(PrefabLStairs, cellWorldPos, Quaternion.identity);
        placedStairs.Add(selectedTilePos, stairs);
        //stairs.transform.localScale = new Vector3(-1f, 1f, 1f);
        map.SetTile(selectedTilePos, TileLStairs);
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void DemolishBlock()
    {
        map.SetTile(selectedTilePos, null);
        // Remove stairs
        placedStairs.TryGetValue(selectedTilePos, out GameObject stairs);
        if (stairs)
        {
            Destroy(stairs);
            placedStairs.Remove(selectedTilePos);
        }
        CellSelection.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    public void PlaySound(AudioClip sound)
    {
        audioSource.clip = sound;
        audioSource.Play();
    }

    void UnselectCell()
    {
        // Remove any active buttons from screen
        interactingWithCell = false;
        CellSelection.SetActive(false);
        UIBreakBlock.SetActive(false);
        UIPlaceUmbrella.SetActive(false);
        UIBuildRStairs.SetActive(false);
        UIBuildLStairs.SetActive(false);
        UIDemolishBlock.SetActive(false);
        UICross.SetActive(false);
        selectedTilePos = Vector3Int.one; // Reset selected tile pos
    }

    /// <summary>
    /// Encode a tile using One-Hot Encoding
    /// </summary>
    /// <param name="tilePos"> Tile to encode </param>
    /// <returns></returns>
    public int[] EncodeTile(Vector3Int tilePos)
    {
        int[] code = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        TileBase tile = map.GetTile(tilePos);

        if (tile == null) { code[0] = 1; } // Empty tile
        else
        {
            if (dataFromTiles.TryGetValue(tile, out TileData tileData))
            {
                if (tileData.structural && !tileData.destructable) { code[1] = 1; } // Hard Tile
                else if (tileData.structural && tileData.destructable) { code[2] = 1; } // Soft Tile
                else if (tileData.damaging) { code[3] = 1; } // Damaging Tile
                else if (tileData.portal) { code[4] = 1; } // Portal Tile
                else if (tileData.highlight) { code[5] = 1; } // Umbrella Tile
                else if (tileData.demolishable)
                {
                    if (placedStairs.TryGetValue(tilePos, out GameObject stairs))
                    {
                        string name = stairs.GetComponent<SpriteRenderer>().sprite.name;
                        if (name.Equals("Stairs Right")) { code[6] = 1; } // Stairs Right Tile
                        else if (name.Equals("Stairs Left")) { code[7] = 1; } // Stairs Left Tile
                    }
                }
            }
        }
        return code;
    }


    void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Victory()
    {
        UIVictory.SetActive(true);
        playerInput = false;
        UnselectCell();
    }

    void Defeat()
    {
        UIDefeat.SetActive(true);
        playerInput = false;
        UnselectCell();
    }

    public void CreatureSaved()
    {
        // Just using 1 creature for now, so call Victory
        if (!agentPlaying)
            Invoke(nameof(Victory), 1f);
        else if (AIAgent)
            AIAgent.LemmingSaved();
    }

    public void CreatureDefeated()
    {
        // Just using 1 creature for now, so call Defeat
        if (!agentPlaying)
            Invoke(nameof(Defeat), 1f);
        else if (AIAgent)
            AIAgent.LemmingKilled();
    }

    public void CheckpointReached()
    {
        if (AIAgent)
            AIAgent.LemmingCheckpoint();
    }

    public Vector2Int GetLemmingPos()
    {
        Vector2Int pos = Vector2Int.zero;
        if (!creature) { creature = GameObject.FindGameObjectWithTag("Creature"); }
        if (creature) { pos = (Vector2Int)map.WorldToCell(creature.transform.position); }
        return pos;
    }


    public void RestartLevel()
    {
        UnselectCell();

        // Reset the map
        CopyTilemap(map, Resources.Load<Tilemap>("Tilemaps/Level 1"));
        CopyTilemap(mapDetail, Resources.Load<Tilemap>("Tilemaps/Level 1 Details"));

        // Respawn the creature
        Destroy(creature);
        creature = spawner.SpawnCreature();

        // Remove umbrellas and stairs
        if (placedUmbrellas.Count > 0)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> entry in placedUmbrellas)
            {
                Destroy(entry.Value);
            }
        }
        if (placedStairs.Count > 0)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> entry in placedStairs)
            {
                Destroy(entry.Value);
            }
        }
        placedUmbrellas.Clear();
        placedStairs.Clear();

        // Reset checkpoints
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        foreach (GameObject point in checkpoints)
        {
            point.GetComponent<Checkpoint>().ResetPoint();
        }
    }

    void CopyTilemap(Tilemap destiny, Tilemap original)
    {
        destiny.ClearAllTiles();
        BoundsInt bounds = original.cellBounds;
        TileBase[] allTiles = original.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    destiny.SetTile(new Vector3Int(x + bounds.x - 1, y + bounds.y - 1, 0), tile);
                }
            }
        }
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
