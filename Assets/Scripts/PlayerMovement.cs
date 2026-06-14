using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("Références")]
    public OxygenManager oxygenManager;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform groundCheck;
    public Rope rope;
    public Grappling grappling;

    [Header("Vitesse & Oxygène")]
    public float moveSpeed = 5f;
    public float minSpeedBonus = 1.0f;
    public float maxSpeedBonus = 2.0f;
    public float sprintMultiplier = 1.7f;

    [Header("Physique")]
    public float acceleration = 15f;
    public float deceleration = 10f;
    public float jumpForce = 10f;
    public float wallJumpForceX = 5f;
    public float wallSlideSpeed = 2f;
    public float groundCheckRadius = 0.2f;
    public LayerMask collisionLayers;
    public LayerMask wallLayers;

    [Header("Swing & Grapple")]
    public float swingForce = 15f;
    public float swingDamping = 0.95f;
    public float grappleForce = 25f;

    // Variables internes
    private bool isSprinting = false;
    private bool isGrappling = false;
    private bool isJumping;
    private bool isGrounded;
    private bool isOnWall;
    private int wallSide;
    private float horizontalMovement;
    private float currentSpeed = 0f;
    public BoxCollider2D leftWallCollider;
    public BoxCollider2D rightWallCollider;
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float footstepTimer;
    private float lastStepTime;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private float footstepInterval = 1.2f;

    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal");

        HandleFootsteps();

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (oxygenManager != null)
        {
            oxygenManager.ManageOxygen(rb.linearVelocity.magnitude);
        }

        if (Input.GetButtonDown("Jump"))
        {
            // Jump au sol
            if (isGrounded)
            {
                isJumping = true;
                coyoteTimeCounter = 0f;
            }
            // Jump en l'air avec coyote time
            else if (coyoteTimeCounter > 0f)
            {
                isJumping = true;
                coyoteTimeCounter = 0f;
            }
            // Wall jump
            else if (isOnWall && !isGrounded)
            {
                isJumping = true;
            }
        }

        // Lancer la corde avec V
        if (Input.GetKeyDown(KeyCode.V))
        {
            LaunchRope();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.instance != null && GameManager.instance.itemGrappin_Ramasse)
            {
                LaunchGrapple();
            }
            else
            {
                Debug.Log("Vous n'avez pas encore récupéré le grappin !");
            }
        }

        if (Input.GetKeyDown(KeyCode.T) && grappling != null && grappling.IsPlanted())
        {
            if (GameManager.instance != null && GameManager.instance.itemGrappin_Ramasse)
            {
                isGrappling = true;
            }
        }

        Flip(rb.linearVelocity.x);

        // LOGS POUR DEBUG
        float speedValue = Mathf.Abs(rb.linearVelocity.x);
        Debug.Log("RB velocity X: " + rb.linearVelocity.x + " | Speed value: " + speedValue + " | Animator null: " + (animator == null));

        if (animator != null)
        {
            animator.SetFloat("Speed", speedValue);
        }
        else
        {
            Debug.LogError("ANIMATOR EST NULL !");
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, collisionLayers);
        DetectWalls();
        MovePlayer(horizontalMovement);
    }



    void MovePlayer(float _horizontalMovement)
    {
        // 1. Calcul du bonus de vitesse basé sur l'oxygène
        // On vérifie que oxygenManager est bien assigné pour éviter les crashs
        float oxygenRatio = (oxygenManager != null) ? oxygenManager.GetOxygenRatio() : 1.0f;
        float speedBoost = Mathf.Lerp(maxSpeedBonus, minSpeedBonus, oxygenRatio);

        // 2. Calcul de la vitesse cible (avec sprint et bonus d'oxygène)
        float targetSpeed = _horizontalMovement * moveSpeed * speedBoost;
        if (isSprinting && _horizontalMovement != 0)
        {
            targetSpeed *= sprintMultiplier;
        }

        // 3. Interpolation de la vitesse horizontale
        if (_horizontalMovement != 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        Vector2 newVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // 4. Traction du grappin
        if (isGrappling && grappling != null && grappling.IsPlanted())
        {
            Vector3 grapplePos = grappling.GetGrapplePosition();
            Vector3 directionToGrapple = (grapplePos - transform.position).normalized;

            if (Vector3.Distance(transform.position, grapplePos) < 1.0f)
            {
                isGrappling = false;
                grappling.RetractGrapple();
            }
            else
            {
                Vector2 grapplePull = (Vector2)directionToGrapple * grappleForce;
                rb.AddForce(grapplePull, ForceMode2D.Force);
                newVelocity.y = rb.linearVelocity.y > 0 ? rb.linearVelocity.y : 0;
            }
        }

        // 5. Contrainte corde (Swing)
        if (rope != null && rope.IsPlanted())
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
            }
        }

        // 6. Gestion du Wall Slide
        if (isOnWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            newVelocity.y = -wallSlideSpeed;
        }

        rb.linearVelocity = newVelocity;

        if (isJumping)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
            else if (isOnWall && !isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                float wallJumpX = wallJumpForceX * wallSide;
                rb.AddForce(new Vector2(wallJumpX, jumpForce), ForceMode2D.Impulse);
                currentSpeed = wallJumpX;
                isOnWall = false;

                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
            else // COYOTE JUMP
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
            isJumping = false;
        }

        // Remet Alt à false quand il touche le sol
        if (isGrounded)
        {
            animator.SetBool("Alt", false);
        }
        else 
        {
            animator.SetBool("Alt", true);
            if (rb.linearVelocityY > 0)
            {
                animator.SetTrigger("Jump");
            }
            else
            {
                animator.SetTrigger("Fall");
            }
        }
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

#if UNITY_EDITOR
        if (rb != null && oxygenManager != null)
        {
            // Calcul de la vitesse actuelle
            float currentSpeedVal = rb.linearVelocity.magnitude;

            // Calcul de la vitesse max théorique en fonction de l'oxygène actuel
            float oxygenRatio = oxygenManager.GetOxygenRatio();
            float speedBoost = Mathf.Lerp(maxSpeedBonus, minSpeedBonus, oxygenRatio);
            float maxPossibleSpeed = moveSpeed * speedBoost * (isSprinting ? sprintMultiplier : 1f);

            // Affichage du texte
            string speedText = "Vitesse: " + currentSpeedVal.ToString("F2") +
                               "\nMax possible: " + maxPossibleSpeed.ToString("F2");

            Handles.Label(transform.position + Vector3.up * 2.5f, speedText);
        }
#endif
    }

    private void HandleFootsteps()
    {
        bool isMoving = isGrounded && Mathf.Abs(horizontalMovement) > 0.1f;

        if (isMoving)
        {
            float currentInterval = isSprinting ? footstepInterval / 1.6f : footstepInterval;

            if (Time.time - lastStepTime > currentInterval)
            {
                AudioSource.PlayClipAtPoint(walkingSound, transform.position, UnityEngine.Random.Range(0.2f, 6f));
                lastStepTime = Time.time;
            }
        }
    }
}