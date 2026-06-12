using System.Linq;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
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

    // À drag-drop dans l'inspecteur
    public BoxCollider2D leftWallCollider;
    public BoxCollider2D rightWallCollider;

    private Vector3 velocity = Vector3.zero;
    private float horizontalMovement;

    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.fixedDeltaTime;
        if (Input.GetButtonDown("Jump") && (isGrounded || (isOnWall && !isGrounded)))
        {
            isJumping = true;
            Debug.Log("Jump - isGrounded: " + isGrounded + " isOnWall: " + isOnWall);
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
        Vector3 targetVelocity = new Vector2(_horizontalMovement, rb.linearVelocity.y);
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, .05f);

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
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(wallJumpForceX * wallSide, jumpForce), ForceMode2D.Impulse);
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