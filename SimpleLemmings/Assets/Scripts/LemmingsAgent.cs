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

    [SerializeField]
    SceneManager sceneManager;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        // If not training mode, no max step, play forever
        if (!trainingMode) { MaxStep = 0; }
        else { sceneManager.playerInput = false; }
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset agent, scene and Lemmings
        sceneManager.RestartLevel();
    }

    /// <summary>
    /// Called when actions are received from either the player input or the neural network
    /// The agent will send one action per cell
    /// 0 Nothing - 1 Break - 2 Umbrella - 3 R Stairs - 4 L Stairs - 5 Demolish Stairs - 6 Remove Umbrella
    /// </summary>
    /// <param name="actions">Buffer of actions received</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> agentActions = actions.DiscreteActions;

        int actionNum = 0;

        for (int h = 0; h < sceneManager.MapHeight; h++)
        {
            for (int w = 0; w < sceneManager.MapWidth; w++)
            {
                if (agentActions[actionNum] != 0 && agentActions[actionNum] <= sceneManager.MaxActions + 1)
                {
                    Vector3Int tilePos = new Vector3Int(w, h, 0);
                    sceneManager.GetTileActions(tilePos, out bool[] allowedActions);
                    // Check that the intended action is allowed
                    if (allowedActions[agentActions[actionNum] - 1] == true)
                    {
                        sceneManager.selectedTilePos = tilePos;
                        if (agentActions[actionNum] == 1) { sceneManager.BreakBlock(); }
                        else if (agentActions[actionNum] == 2) { sceneManager.PlaceUmbrella(); }
                        else if (agentActions[actionNum] == 3) { sceneManager.BuildRStairs(); }
                        else if (agentActions[actionNum] == 4) { sceneManager.BuildLStairs(); }
                        else if (agentActions[actionNum] == 5) { sceneManager.DemolishBlock(); }
                        else if (agentActions[actionNum] == 6) { sceneManager.RemoveUmbrella(); }
                    }
                }

                actionNum++;
            }
        }
    }

    /// <summary>
    /// Collect information from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the lemming position
        sensor.AddObservation(sceneManager.GetLemmingPos());

        // Observe the tilemap
        for (int h = 0; h < sceneManager.MapHeight; h++)
        {
            for (int w = 0; w < sceneManager.MapWidth; w++)
            {
                // Encode tile
                int[] tileCode = sceneManager.EncodeTile(new Vector3Int(w, h, 0));

                // Send each value of the code as an observation
                for (int i = 0; i < tileCode.Length; i++)
                {
                    sensor.AddObservation(tileCode[i]);
                }
            }
        }
    }

    /// <summary>
    /// Mask unavailable actions for each cell in the level
    /// The agent will only be able to perform available actions on each decision
    /// </summary>
    /// <param name="actionMask"></param>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        int branch = 0;

        for (int h = 0; h < sceneManager.MapHeight; h++)
        {
            for (int w = 0; w < sceneManager.MapWidth; w++)
            {
                Vector3Int tilePos = new Vector3Int(w, h, 0);
                sceneManager.GetTileActions(tilePos, out bool[] allowedActions);

                List<int> unavailableActions = new List<int>();

                for (int action = 0; action < allowedActions.Length; action++)
                {
                    if (allowedActions[action] == false)
                        unavailableActions.Add(action + 1);
                }

                actionMask.WriteMask(branch, unavailableActions);

                branch++;
            }
        }
    }

    /// <summary>
    /// Reward the agent for every saved Lemming
    /// </summary>
    public void LemmingSaved()
    {
        if (trainingMode)
            AddReward(1f);
    }

    /// <summary>
    /// Punish the agent if a Lemming dies
    /// </summary>
    public void LemmingKilled()
    {
        if (trainingMode)
            AddReward(-1f);
    }

    /// <summary>
    /// Slightly reward the agent when reaching a checkpoint
    /// </summary>
    public void LemmingCheckpoint()
    {
        if (trainingMode)
            AddReward(0.2f);
    }
}
