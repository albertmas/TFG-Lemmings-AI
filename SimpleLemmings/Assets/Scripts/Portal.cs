using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Creature"))
        {
            Destroy(other.gameObject);
            GameObject.Find("SceneManager").GetComponent<SceneManager>().CreatureSaved();
        }
    }
}
