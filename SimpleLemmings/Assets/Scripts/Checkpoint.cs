using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool activated = false;
    SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated && other.CompareTag("Creature"))
        {
            activated = true;
            sprite.color = Color.gray;
            GameObject.Find("SceneManager").GetComponent<SceneManager>().CheckpointReached();
        }
    }

    public void ResetPoint()
    {
        activated = false;
        sprite.color = Color.white;
    }
}
