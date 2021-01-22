using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    public float fallingGravity = 3.0f;
    bool goingRight = true;
    bool isGrounded = false;
    readonly float deadlyHeight = 2.1f;
    Vector2 fallOrigin;
    bool hasUmbrella = false;
    public bool climbingSlope = false;

    public bool IsAlive { get; private set; } = true;

    Rigidbody2D creatureRigidbody;
    SceneManager sceneManager;
    public AudioClip hitGround;
    protected Animator anim;
    //public BoxCollider2D GroundTrigger;

    // Start is called before the first frame update
    void Start()
    {
        creatureRigidbody = GetComponent<Rigidbody2D>();
        sceneManager = FindObjectOfType<SceneManager>();
        anim = transform.Find("model").GetComponent<Animator>();

        fallOrigin = transform.position;
        anim.Play("Run");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) { return; }

        if (isGrounded)
        {
            // Creature is walking
            int direction = goingRight ? 1 : -1;
            transform.Translate(Vector3.right * movementSpeed * direction * Time.deltaTime);
            if (climbingSlope)
                transform.Translate(Vector3.up * movementSpeed * Time.deltaTime);
            //creatureRigidbody.AddForce(Vector2.right * direction * 5.0f);
            //float clampedSpeed = Mathf.Clamp(creatureRigidbody.velocity.x, -movementSpeed, movementSpeed);
            //creatureRigidbody.velocity = new Vector2(clampedSpeed, creatureRigidbody.velocity.y);
        }
        else
        {
            // Creature is falling
            creatureRigidbody.AddForce(Vector3.down * fallingGravity); // Push down
            if (hasUmbrella) // Cap max Y velocity if creature has an umbrella
            {
                float clampedSpeed = Mathf.Clamp(creatureRigidbody.velocity.y, -movementSpeed, movementSpeed);
                creatureRigidbody.velocity = new Vector2(creatureRigidbody.velocity.x, clampedSpeed);
            }
        }

        if (sceneManager.CheckForDamagingTile(transform.position - new Vector3(0f, 1f, 0f)))
        {
            Die();
        }
    }

    public void Turn()
    {
        goingRight = !goingRight;
        Vector3 creatureScale = transform.localScale;
        creatureScale = new Vector3(goingRight ? Mathf.Abs(creatureScale.x) : -Mathf.Abs(creatureScale.x), creatureScale.y, creatureScale.z); // TO FIX
        transform.localScale = creatureScale; // new Vector3(goingRight ? 1 : -1, 1, 1);
        creatureRigidbody.velocity = new Vector2(0f, creatureRigidbody.velocity.y);
    }

    public void Fall()
    {
        anim.Play("Jump");
        fallOrigin = transform.position;
        isGrounded = false;
    }

    public void Land()
    {
        if (!isGrounded)
        {
            Vector2 landingPos = transform.position;
            float fallHeight = fallOrigin.y - landingPos.y;

            if (hasUmbrella)
            {
                hasUmbrella = false;
                //creatureRigidbody.gravityScale = 3;
            }
            else if (Mathf.Abs(fallHeight) >= deadlyHeight)
            {
                Die();
                return;
            }

            anim.Play("Run");
            isGrounded = true;
        }
    }

    public void EquipUmbrella()
    {
        hasUmbrella = true;
        //creatureRigidbody.gravityScale = .05f;
    }

    void Die()
    {
        anim.Play("Die");
        IsAlive = false;
        sceneManager.PlaySound(hitGround);
        sceneManager.CreatureDefeated();
        //Destroy(gameObject, 2f);
    }
}
