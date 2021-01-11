using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeDetector : MonoBehaviour
{
    CreatureMovement creatureMovement;
    Rigidbody2D creatureRigidbody;

    private void Start()
    {
        creatureMovement = transform.parent.GetComponent<CreatureMovement>();
        creatureRigidbody = transform.parent.GetComponent<Rigidbody2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Stairs") || other.CompareTag("ColliderTilemap"))
        {
            creatureMovement.climbingSlope = true;
            //creatureRigidbody.gravityScale = 0.002f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stairs") || other.CompareTag("ColliderTilemap"))
        {
            creatureMovement.climbingSlope = false;
            //creatureRigidbody.gravityScale = 3f;
        }
    }
}
