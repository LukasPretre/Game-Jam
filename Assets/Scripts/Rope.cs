using UnityEngine;

public class Rope : MonoBehaviour
{
    public Transform playerTransform;
    public float ropeSpeed = 20f;
    public float ropeGravity = 9.81f;
    public float maxRopeLength = 15f; // NOUVEAU - taille max de la corde
    public LayerMask ropeCollisionLayers;

    private Vector3 ropePos;
    private Vector3 ropeDirection;
    private Vector3 ropeVelocity;
    private bool isMoving = false;
    private bool isPlanted = false;
    private float ropeLength = 0f;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerMovement>().transform;
            Debug.Log("PlayerTransform trouvé automatiquement: " + playerTransform.name);
        }
    }

    void Update()
    {
        if (isMoving && !isPlanted)
        {
            ropeVelocity.y -= ropeGravity * Time.deltaTime;
            ropePos += ropeVelocity * Time.deltaTime;

            ropeLength = Vector3.Distance(playerTransform.position, ropePos);

            Debug.Log("Corde position: " + ropePos + " | Distance: " + ropeLength);

            Vector3 raycastDirection = (ropePos - playerTransform.position).normalized;
            float raycastDistance = Vector3.Distance(playerTransform.position, ropePos) + 1f;

            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, raycastDirection, raycastDistance, ropeCollisionLayers);

            if (hit.collider != null)
            {
                Debug.Log("Hit détecté! Objet: " + hit.collider.name + " Tag: " + hit.collider.tag);

                if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Sol"))
                {
                    ropePos = hit.point;
                    isPlanted = true;
                    isMoving = false;
                    Debug.Log("Corde plantée sur: " + hit.collider.name + " à " + ropePos);
                }
                else
                {
                    Debug.Log("Hit pas bon tag. Tag trouvé: " + hit.collider.tag);
                }
            }

            if (ropeLength > maxRopeLength) // MODIFIÉ - utilise maxRopeLength
            {
                Debug.Log("Corde trop loin, rétractée");
                RetractRope();
            }
        }
    }

    public void LaunchRope(Vector3 direction)
    {
        ropePos = playerTransform.position;
        ropeVelocity = direction.normalized * ropeSpeed;
        isMoving = true;
        isPlanted = false;
        ropeLength = 0f;
        Debug.Log("Corde lancée vers: " + direction);
    }

    public void RetractRope()
    {
        isMoving = false;
        isPlanted = false;
        ropeLength = 0f;
        Debug.Log("Corde rétractée");
    }

    public Vector3 GetRopePosition()
    {
        return ropePos;
    }

    public bool IsPlanted()
    {
        return isPlanted;
    }

    public float GetRopeLength()
    {
        return ropeLength;
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (isMoving || isPlanted)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerTransform.position, ropePos);

            Gizmos.color = isPlanted ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(ropePos, 0.3f);
            Gizmos.DrawSphere(ropePos, 0.15f);
        }
    }
}