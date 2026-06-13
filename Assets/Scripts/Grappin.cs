using UnityEngine;

public class Grappling : MonoBehaviour
{
    public Transform playerTransform;
    public float grappleSpeed = 50f; // Très rapide, pas de physique
    public LayerMask grapplingCollisionLayers;

    private Vector3 grapplePos;
    private Vector3 grappleDirection;
    private bool isMoving = false;
    private bool isPlanted = false;
    private float grapplingDistance = 0f;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerMovement>().transform;
        }
    }

    void Update()
    {
        if (isMoving && !isPlanted)
        {
            // 1. Calculer la position future
            Vector3 nextPos = grapplePos + (grappleDirection * grappleSpeed * Time.deltaTime);

            // 2. Faire un Raycast entre la position actuelle et la future position
            float distToNext = Vector3.Distance(grapplePos, nextPos);
            RaycastHit2D hit = Physics2D.Raycast(grapplePos, grappleDirection, distToNext, grapplingCollisionLayers);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Sol"))
                {
                    grapplePos = hit.point;
                    isPlanted = true;
                    isMoving = false;
                }
            }
            else
            {
                // 3. Si pas de collision, on avance la position
                grapplePos = nextPos;
            }

            grapplingDistance = Vector3.Distance(playerTransform.position, grapplePos);

            if (grapplingDistance > 30f)
            {
                RetractGrapple();
            }
        }
    }

    public void LaunchGrapple(Vector3 direction)
    {
        grapplePos = playerTransform.position;
        grappleDirection = direction.normalized;
        isMoving = true;
        isPlanted = false;
        grapplingDistance = 0f;
        Debug.Log("Grappin lancé vers: " + direction);
    }

    public void RetractGrapple()
    {
        isMoving = false;
        isPlanted = false;
        grapplingDistance = 0f;
        Debug.Log("Grappin rétracté");
    }

    public Vector3 GetGrapplePosition()
    {
        return grapplePos;
    }

    public bool IsPlanted()
    {
        return isPlanted;
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        if (isMoving || isPlanted)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerTransform.position, grapplePos);

            Gizmos.color = isPlanted ? Color.blue : Color.cyan;
            Gizmos.DrawWireSphere(grapplePos, 0.2f);
        }
    }
}