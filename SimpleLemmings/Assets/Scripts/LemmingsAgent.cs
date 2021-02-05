using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A Lemmings Machine Learning Agent
/// </summary>
public class LemmingsAgent : Agent
{
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    public SceneManager sceneManager;
    public GameObject lemming;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset agent, scene and Lemmings
    }

    /// <summary>
    /// Called when and action is received from either the player input or the neural network
    /// </summary>
    /// <param name="actions">Buffer of actions received</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // If lemming is null, observe an empty array and return
        if (lemming == null)
        {
            sensor.AddObservation(new float[2]);
            return;
        }

        // Observe the lemming position
        sensor.AddObservation((Vector2Int)sceneManager.map.WorldToCell(lemming.transform.position));

        // Observe the tilemap
        for (int h = 0; h < sceneManager.MapHeight; h++)
        {
            for (int w = 0; w < sceneManager.MapWidth; w++)
            {
                Vector3Int tilePos = new Vector3Int(w, h, 0);
                TileBase tile = sceneManager.map.GetTile(tilePos);
                sceneManager.GetTileActions(tilePos, out bool[] actions);
            }
        }
    }
}
