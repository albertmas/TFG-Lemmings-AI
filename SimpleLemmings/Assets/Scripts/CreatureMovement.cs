using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    bool goingRight = true;
    public bool isGrounded = false;
    bool hasUmbrella = false;

    Rigidbody2D rigidbody;
    SceneManager sceneManager;
    //public BoxCollider2D GroundTrigger;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody.GetComponent<Rigidbody2D>();
        sceneManager = FindObjectOfType<SceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        int direction = goingRight ? 1 : -1;
        transform.Translate(Vector3.right * movementSpeed * direction * Time.deltaTime);

        if (sceneManager.CheckForDamagingTile(transform.position - new Vector3(0f, 1f, 0f)))
        {
            Destroy(gameObject);
        }
    }

    public void Turn()
    {
        goingRight = !goingRight;
        Vector3 creatureScale = transform.localScale;
        creatureScale = new Vector3(goingRight ? Mathf.Abs(creatureScale.x) : -Mathf.Abs(creatureScale.x), creatureScale.y, creatureScale.z); // TO FIX
        transform.localScale = creatureScale; // new Vector3(goingRight ? 1 : -1, 1, 1);
    }

    public void UseUmbrella()
    {
        hasUmbrella = true;
        //rigidbody.gravityScale = 1;
    }
}
