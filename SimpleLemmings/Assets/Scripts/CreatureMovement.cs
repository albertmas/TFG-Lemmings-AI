using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureMovement : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    bool goingRight = true;

    SceneManager SceneManager;
    //public BoxCollider2D GroundTrigger;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager = FindObjectOfType<SceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        int direction = goingRight ? 1 : -1;
        transform.Translate(Vector3.right * movementSpeed * direction * Time.deltaTime);

        if (SceneManager.CheckForDamagingTile(transform.position - new Vector3(0f, 1f, 0f)))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        goingRight = !goingRight;
        transform.localScale = new Vector3(goingRight ? -1 : 1, 1, 1);
    }
}
