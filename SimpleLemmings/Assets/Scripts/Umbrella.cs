using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Umbrella : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Creature")
        {
            collision.gameObject.GetComponent<CreatureMovement>().UseUmbrella();
        }
    }
}
