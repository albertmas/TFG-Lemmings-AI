using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// A Lemmings Machine Learning Agent
/// </summary>
public class LemmingsAgentNew : Agent
{
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    [Tooltip("Number of decisions allowed to the agent per episode")]
    public int decisions = 10;

    [Tooltip("Whether the agent is rewarded when getting close to the portal")]
    public bool distanceReward;

    [Tooltip("Number of steps from the spawn point to the portal")]
    public int distanceSteps = 10;

    [Tooltip("Agent reward per step")]
    public float stepReward = 0.1f;

    int decisionsLeft = 0;

    [SerializeField]
    SceneManager sceneManager;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        // If not training mode, no max step, play forever
        if (!trainingMode)
        {
            MaxStep = 0;
            sceneManager.playerInput = false;
        }

        // Agent is playing the game
        sceneManager.agentPlaying = true;

        // If BehaviourType is not Heuristic, cancel player input
        if (GetComponent<BehaviorParameters>().BehaviorType != BehaviorType.HeuristicOnly)
            sceneManager.playerInput = false;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset agent, scene and Lemmings
        sceneManager.RestartLevel();

        decisionsLeft = decisions;
    }

    void Update()
    {
        if (decisionsLeft > 0)
            RequestDecision();

        //if (StepCount % MaxStep == 0)
        //    RewardDistanceToPortal();
    }

    /// <summary>
    /// Called when actions are received from either the player input or the neural network
    /// The agent will choose a cell and an action
    /// Branch 0 -> Cell X pos
    /// Branch 1 -> Cell Y pos
    /// Branch 2 -> Action
    /// 0 Nothing - 1 Break - 2 Umbrella - 3 R Stairs - 4 L Stairs
    /// </summary>
    /// <param name="actions">Buffer of actions received</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> agentActions = actions.DiscreteActions;

        if (agentActions[2] != 0)
        {
            Vector3Int tilePos = new Vector3Int(agentActions[0], agentActions[1], 0);
            sceneManager.GetTileActions(tilePos, out bool[] allowedActions);
            // Check that the intended action is allowed
            if (allowedActions[agentActions[2] - 1] == true)
            {
                sceneManager.selectedTilePos = tilePos;
                if (agentActions[2] == 1) { sceneManager.BreakBlock(); }
                else if (agentActions[2] == 2) { sceneManager.PlaceUmbrella(); }
                else if (agentActions[2] == 3) { sceneManager.BuildRStairs(); }
                else if (agentActions[2] == 4) { sceneManager.BuildLStairs(); }
                else { Debug.LogWarning("Unknown action requested by agent"); }

                //AddReward(-0.1f); // Slightly penalise the agent for each action
                //AddReward(0.2f); // Slightly reward the agent for performing a correct action
                decisionsLeft--;
            }
            else
            {
                //AddReward(-0.2f); // If the action is not possible, penalize the agent
                //Debug.Log("Invalid action!");
                decisionsLeft--;
            }
        }
        else // Agent chose to do nothing
        {
            //AddReward(0.2f); // Slightly reward the agent for performing a correct action
            decisionsLeft--;
            //Debug.Log("Action skipped!");
        }
    }

    /// <summary>
    /// Collect information from the environment
    /// 899 Total Observations
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the number of decisions left
        sensor.AddObservation(decisionsLeft); // 1 Observation

        // Observe the lemming position
        sensor.AddObservation(sceneManager.GetLemmingPos()); // 2 Observations

        // Observe the tilemap
        for (int h = 0; h < sceneManager.MapHeight; h++) // 8 Tiles High
        {
            for (int w = 0; w < sceneManager.MapWidth; w++) // 16 Tiles Wide
            {
                // Encode tile
                int[] tileCode = sceneManager.EncodeTile(new Vector3Int(w, h, 0)); // 7 Digit Code

                // Send each value of the code as an observation
                for (int i = 0; i < tileCode.Length; i++)
                {
                    sensor.AddObservation(tileCode[i]);
                }
            }
            // 896 Observations
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int action = 0;

        if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Keypad1)) { action = 1; }
        else if (Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Keypad2)) { action = 2; }
        else if (Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Keypad3)) { action = 3; }
        else if (Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Keypad4)) { action = 4; }

        actionsOut.DiscreteActions.Array[0] = sceneManager.selectedTilePos.x;
        actionsOut.DiscreteActions.Array[1] = sceneManager.selectedTilePos.y;
        actionsOut.DiscreteActions.Array[2] = action;
    }

    /// <summary>
    /// Reward the agent for every saved Lemming
    /// </summary>
    public void LemmingSaved()
    {
        if (trainingMode)
            AddReward(1f);

        // We only have 1 Lemming, so call end episode
        EndEpisode();
    }

    /// <summary>
    /// Punish the agent if a Lemming dies
    /// </summary>
    public void LemmingKilled()
    {
        if (trainingMode)
            AddReward(-1f);

        // We only have 1 Lemming, so call end episode
        EndEpisode();
    }

    /// <summary>
    /// Slightly reward the agent when reaching a checkpoint
    /// </summary>
    public void LemmingCheckpoint()
    {
        if (trainingMode)
            AddReward(0.2f);
    }

    /// <summary>
    /// Slightly reward the agent when getting closer to the portal
    /// </summary>
    public void RewardDistanceToPortal()
    {
        Debug.Log("distance reward");
        AddReward(stepReward);
    }
}
