using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float acceleration = 15f;
    public float deceleration = 10f;
    public bool isJumping;
    public float jumpForce;
    public float wallJumpForceX;
    public bool isGrounded;
    public bool isOnWall;
    public int wallSide;
    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask collisionLayers;
    public LayerMask wallLayers;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public float wallSlideSpeed = 2f;

    public BoxCollider2D leftWallCollider;
    public BoxCollider2D rightWallCollider;

    // Rope
    public Rope rope;
    public float swingForce = 15f;
    public float swingDamping = 0.95f;

    // NOUVEAU - Grappling
    public Grappling grappling;
    public float grappleForce = 25f; // Force de traction vers le grappin
    private float grapplingTimer = 0f; // Timer de détachement auto (3 secondes)
    private bool isGrappling = false;

    private Vector3 velocity = Vector3.zero;
    private float horizontalMovement;
    private float currentSpeed = 0f;

    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump") && (isGrounded || (isOnWall && !isGrounded)))
        {
            isJumping = true;
            Debug.Log("Jump - isGrounded: " + isGrounded + " isOnWall: " + isOnWall);
        }

        // Lancer la corde avec V
        if (Input.GetKeyDown(KeyCode.V))
        {
            LaunchRope();
        }

        // NOUVEAU - Lancer le grappin avec R
        if (Input.GetKeyDown(KeyCode.R))
        {
            LaunchGrapple();
        }

        // NOUVEAU - Accrocher avec T
        if (Input.GetKey(KeyCode.T) && grappling.IsPlanted() && !isGrounded)
        {
            isGrappling = true;
            grapplingTimer += Time.deltaTime;
            Debug.Log("Grappling! Timer: " + grapplingTimer);

            // Détachement auto après 3 secondes
            if (grapplingTimer >= 3f)
            {
                isGrappling = false;
                grapplingTimer = 0f;
                grappling.RetractGrapple();
                Debug.Log("Grappin détaché automatiquement!");
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.T))
            {
                isGrappling = false;
                grapplingTimer = 0f;
                Debug.Log("Grappin libéré");
            }
        }

        Flip(rb.linearVelocity.x);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, collisionLayers);
        DetectWalls();
        MovePlayer(horizontalMovement);
    }

    void LaunchRope()
    {
        Vector3 ropeDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;
        Debug.Log("LaunchRope appelé, direction: " + ropeDirection);
        rope.LaunchRope(ropeDirection);
    }

    // NOUVEAU - Lancer le grappin
    void LaunchGrapple()
    {
        Vector3 grappleDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;
        Debug.Log("LaunchGrapple appelé, direction: " + grappleDirection);
        grappling.LaunchGrapple(grappleDirection);
    }

    void DetectWalls()
    {
        if (leftWallCollider == null || rightWallCollider == null) return;

        ColliderArray2D leftWallDetect = Physics2D.OverlapCollider(leftWallCollider, new ContactFilter2D { layerMask = wallLayers });
        ColliderArray2D rightWallDetect = Physics2D.OverlapCollider(rightWallCollider, new ContactFilter2D { layerMask = wallLayers });

        if (leftWallDetect.Length > 0)
        {
            isOnWall = true;
            wallSide = 1;
            Debug.Log("Mur GAUCHE");
        }
        else if (rightWallDetect.Length > 0)
        {
            isOnWall = true;
            wallSide = -1;
            Debug.Log("Mur DROITE");
        }
        else
        {
            isOnWall = false;
            wallSide = 0;
        }
    }

    void MovePlayer(float _horizontalMovement)
    {
        if (_horizontalMovement != 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, _horizontalMovement * moveSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        Vector2 newVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // NOUVEAU - Traction du grappin
        if (isGrappling && grappling.IsPlanted())
        {
            Vector3 directionToGrapple = (grappling.GetGrapplePosition() - transform.position).normalized;
            Vector2 grapplePull = (Vector2)directionToGrapple * grappleForce;
            rb.AddForce(grapplePull, ForceMode2D.Force);

            // Pas de chute en grappling
            newVelocity.y = 0;
            Debug.Log("Grapple Pull!");
        }

        // Contrainte rope
        if (rope.IsPlanted())
        {
            Vector3 ropePos = rope.GetRopePosition();
            Vector3 playerPos = transform.position;
            Vector3 directionToRope = (ropePos - playerPos).normalized;
            float distanceToRope = Vector3.Distance(playerPos, ropePos);

            if (distanceToRope >= rope.maxRopeLength)
            {
                Vector3 directionFromRope = -directionToRope;

                float velocityAwayFromRope = Vector2.Dot(newVelocity, (Vector2)directionFromRope);

                if (velocityAwayFromRope > 0.01f)
                {
                    newVelocity -= (Vector2)directionFromRope * velocityAwayFromRope;
                }

                Vector3 tangentDirection = Vector3.Cross(directionFromRope, Vector3.forward).normalized;
                float tangentialVelocity = Vector2.Dot(newVelocity, (Vector2)tangentDirection);

                tangentialVelocity *= swingDamping;
                newVelocity = (Vector2)tangentDirection * tangentialVelocity;

                Vector2 tensionForce = (Vector2)directionFromRope * swingForce;
                rb.AddForce(tensionForce, ForceMode2D.Force);

                Debug.Log("Swing! Distance: " + distanceToRope);
            }
        }

        if (isOnWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            newVelocity.y = -wallSlideSpeed;
        }

        rb.linearVelocity = newVelocity;

        if (isJumping)
        {
            if (isGrounded)
            {
                Debug.Log("Saut SOL");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            }
            else if (isOnWall && !isGrounded)
            {
                Debug.Log("Saut MUR - Direction: " + wallSide);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

                float wallJumpX = wallJumpForceX * wallSide;
                rb.AddForce(new Vector2(wallJumpX, jumpForce), ForceMode2D.Impulse);

                currentSpeed = wallJumpX;

                isOnWall = false;
            }

            isJumping = false;
        }
    }

    void Flip(float _velocity)
    {
        if (_velocity > 0.1f)
            spriteRenderer.flipX = false;
        else if (_velocity < -0.1f)
            spriteRenderer.flipX = true;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}