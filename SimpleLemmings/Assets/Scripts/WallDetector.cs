using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour
{
    CreatureMovement CreatureMovement;

    void Start()
    {
        CreatureMovement = transform.root.GetComponent<CreatureMovement>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ColliderTilemap"))
        {
            CreatureMovement.Turn();
        }
    }
}
