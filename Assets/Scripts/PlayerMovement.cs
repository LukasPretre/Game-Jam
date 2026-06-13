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

    // NOUVEAU - Rope
    public Rope rope;
    public float swingForce = 15f; // NOUVEAU - force du swing (à ajuster dans l'inspecteur)

    private Vector3 velocity = Vector3.zero;
    private float horizontalMovement;
    private float currentSpeed = 0f;

    public float swingDamping = 0.95f; // Friction du swing (ajuste dans l'inspecteur: 0.90 à 0.98)

    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Jump") && (isGrounded || (isOnWall && !isGrounded)))
        {
            isJumping = true;
            Debug.Log("Jump - isGrounded: " + isGrounded + " isOnWall: " + isOnWall);
        }

        // NOUVEAU - Lancer la corde avec V
        if (Input.GetKeyDown(KeyCode.V))
        {
            LaunchRope();
        }

        Flip(rb.linearVelocity.x);
        // MODIFIÉ - sans HasParameter
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, collisionLayers);
        DetectWalls();
        MovePlayer(horizontalMovement);
    }

    // NOUVEAU - Lancer la corde
    void LaunchRope()
    {
        Vector3 ropeDirection = spriteRenderer.flipX ? Vector3.left : Vector3.right;
        Debug.Log("LaunchRope appelé, direction: " + ropeDirection);
        rope.LaunchRope(ropeDirection);
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

        // MODIFIÉ - contrainte de distance avec swing amélioré
        if (rope.IsPlanted())
        {
            Vector3 ropePos = rope.GetRopePosition();
            Vector3 playerPos = transform.position;
            Vector3 directionToRope = (ropePos - playerPos).normalized;
            float distanceToRope = Vector3.Distance(playerPos, ropePos);

            if (distanceToRope >= rope.maxRopeLength)
            {
                Vector3 directionFromRope = -directionToRope;

                // Calcule la vélocité qui s'éloigne de la corde
                float velocityAwayFromRope = Vector2.Dot(newVelocity, (Vector2)directionFromRope);

                // Si tu t'éloignes, on l'annule
                if (velocityAwayFromRope > 0.01f)
                {
                    newVelocity -= (Vector2)directionFromRope * velocityAwayFromRope;
                }

                // NOUVEAU - Calcule la composante TANGENTIELLE (le long du swing)
                Vector3 tangentDirection = Vector3.Cross(directionFromRope, Vector3.forward).normalized;
                float tangentialVelocity = Vector2.Dot(newVelocity, (Vector2)tangentDirection);

                // Applique la friction UNIQUEMENT au mouvement tangentiel
                tangentialVelocity *= swingDamping;
                newVelocity = (Vector2)tangentDirection * tangentialVelocity;

                // Applique la force de swing
                Vector2 tensionForce = (Vector2)directionFromRope * swingForce;
                rb.AddForce(tensionForce, ForceMode2D.Force);

                Debug.Log("Swing! Distance: " + distanceToRope + " | TangentialVel: " + tangentialVelocity);
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