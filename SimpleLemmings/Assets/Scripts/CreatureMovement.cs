using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    public float movementSpeed = 1.0f;
    public float fallingGravity = 12.0f;
    public float floatHeight = 0.2f;
    public float liftForce = 3.0f;
    public float liftDamping = 3.0f;
    bool goingRight = true;
    bool isGrounded = false;
    readonly float deadlyHeight = 2.1f;
    Vector2 fallOrigin;
    bool hasUmbrella = false;
    public bool climbingSlope = false;

    public bool IsAlive { get; private set; } = true;

    Rigidbody2D rbody;
    SceneManager sceneManager;
    public AudioClip hitGround;
    protected Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        sceneManager = FindObjectOfType<SceneManager>();
        anim = transform.Find("model").GetComponent<Animator>();

        fallOrigin = transform.position;
    }

    void Start()
    {
        anim.Play("Run");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) { return; }

        // Kill creature if it falls through the map
        if (transform.position.y < 0f) { Die(); }

        if (sceneManager.CheckForDamagingTile(transform.position - new Vector3(0f, 1f, 0f)))
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        if (IsAlive)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 0.5f, Color.green);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, LayerMask.GetMask("Map"));
            if (hit.collider != null)
            {
                if (!isGrounded) { Land(); }

                // Walk
                int direction = goingRight ? 1 : -1;
                rbody.velocity = new Vector2(movementSpeed * direction, rbody.velocity.y);

                // Hover over the ground
                float distance = Mathf.Abs(hit.point.y - transform.position.y);
                float heightError = floatHeight - distance;
                float force = liftForce * heightError / floatHeight - rbody.velocity.y * liftDamping;

                if (hit.collider.CompareTag("Stairs")) // Push harder on stairs
                    force *= 2.0f;

                rbody.AddForce(Vector2.up * force); // Push up
            }
            else if (isGrounded)
            {
                Fall();
            }
        }

        if (!isGrounded)
        {
            // Creature is falling
            rbody.AddForce(Vector2.down * fallingGravity); // Push down
            if (hasUmbrella) // Cap max Y velocity if creature has an umbrella
            {
                float clampedSpeed = Mathf.Clamp(rbody.velocity.y, -movementSpeed, movementSpeed);
                rbody.velocity = new Vector2(rbody.velocity.x * 0.95f, clampedSpeed); // Cap Y vel and slowly reduce X vel
            }
        }
    }

    public void Turn()
    {
        goingRight = !goingRight;
        Vector3 creatureScale = transform.localScale;
        creatureScale = new Vector3(goingRight ? Mathf.Abs(creatureScale.x) : -Mathf.Abs(creatureScale.x), creatureScale.y, creatureScale.z); // TO FIX
        transform.localScale = creatureScale; // new Vector3(goingRight ? 1 : -1, 1, 1);
        rbody.velocity = new Vector2(0f, rbody.velocity.y);
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
    }

    void Die()
    {
        anim.Play("Die");
        IsAlive = false;
        sceneManager.PlaySound(hitGround);
        sceneManager.CreatureDefeated();
    }
}
