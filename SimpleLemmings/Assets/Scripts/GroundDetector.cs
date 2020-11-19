using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    CreatureMovement CreatureMovement;

    void Start()
    {
        CreatureMovement = transform.root.GetComponent<CreatureMovement>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ColliderTilemap"))
        {
            CreatureMovement.isGrounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ColliderTilemap"))
        {
            CreatureMovement.isGrounded = false;
        }
    }
}
