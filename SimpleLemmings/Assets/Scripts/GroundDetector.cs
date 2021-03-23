using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    int triggerCount = 0;

    CreatureMovement CreatureMovement;

    void Awake()
    {
        CreatureMovement = transform.root.GetComponent<CreatureMovement>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ColliderTilemap") || other.CompareTag("Stairs"))
        {
            triggerCount++;
            if (triggerCount == 1 && CreatureMovement.IsAlive)
                CreatureMovement.Land();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ColliderTilemap") || other.CompareTag("Stairs"))
        {
            triggerCount--;
            if (triggerCount == 0 && CreatureMovement.IsAlive)
                CreatureMovement.Fall();
        }
    }
}
