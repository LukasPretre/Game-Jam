using UnityEngine;

public class Grappling : MonoBehaviour
{
    public Transform playerTransform;
    public float grappleSpeed = 50f;
    public LayerMask grapplingCollisionLayers;

    private Vector3 grapplePos;
    private Vector3 grappleDirection;
    private bool isMoving = false;
    private bool isPlanted = false;
    private float grapplingDistance = 0f;

    // Variables pour la visualisation
    private LineRenderer lineRenderer;
    private GameObject grappleSphere;

    // ✨ COULEURS PERSONNALISÉES
    private Color colorLaunch = new Color(0.82f, 0.76f, 0.59f);   // D2C296
    private Color colorPlanted = new Color(0.50f, 0.77f, 0.84f);  // 7FC4D5

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerMovement>().transform;
        }

        // Initialiser le LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.sortingOrder = 10;
        lineRenderer.enabled = false;

        // Créer la sphère visuelle
        grappleSphere = new GameObject("GrappleSphere");
        grappleSphere.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("WhiteCircle");
        grappleSphere.transform.parent = transform;
        grappleSphere.SetActive(false);
    }

    void Update()
    {
        if (isMoving && !isPlanted)
        {
            Vector3 nextPos = grapplePos + (grappleDirection * grappleSpeed * Time.deltaTime);
            float distToNext = Vector3.Distance(grapplePos, nextPos);
            RaycastHit2D hit = Physics2D.Raycast(grapplePos, grappleDirection, distToNext, grapplingCollisionLayers);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Sol") || hit.collider.CompareTag("grab"))
                {
                    grapplePos = hit.point;
                    isPlanted = true;
                    isMoving = false;
                }
            }
            else
            {
                grapplePos = nextPos;
            }

            grapplingDistance = Vector3.Distance(playerTransform.position, grapplePos);

            if (grapplingDistance > 30f)
            {
                RetractGrapple();
            }
        }

        // Mettre à jour la visualisation avec les nouvelles couleurs
        if (isMoving || isPlanted)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, playerTransform.position);
            lineRenderer.SetPosition(1, grapplePos);

            // ✨ UTILISER LES COULEURS APPROPRIÉES
            Color currentColor = isPlanted ? colorPlanted : colorLaunch;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            grappleSphere.SetActive(true);
            grappleSphere.transform.position = grapplePos;
            grappleSphere.GetComponent<SpriteRenderer>().color = currentColor;
        }
        else
        {
            lineRenderer.enabled = false;
            grappleSphere.SetActive(false);
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
}