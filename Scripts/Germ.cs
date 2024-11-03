using System.Collections;
using UnityEngine;

public class Germ : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;

    // Movement parameters
    private float moveSpeed;
    private float turnSpeed;
    public float minSpeed = 2f; // Minimum speed
    public float maxSpeed = 4f; // Maximum speed

    // Movement bounds
    private float cylinderRadius;
    private float lowerBoundY;
    private float upperBoundY;

    // Path control points for curved movement
    private Vector3 startPoint;
    private Vector3 controlPoint1;
    private Vector3 controlPoint2;
    private Vector3 endPoint;
    private float t; // Interpolation factor
    private float curveLength; // Length of the Bezier curve

    // Renderer for the germ's visual appearance
    public Renderer germRenderer;

    private ParticleSystem deathEffect;
    private bool isDead = false;

    void Start()
    {
        // Find the GermAreaManager from the scene
        GameObject managerObject = GameObject.FindGameObjectWithTag("pointManager");
        if (managerObject != null)
        {
            GermAreaManager manager = managerObject.GetComponent<GermAreaManager>();
            GermAreaManager.LowerPoint = manager.lowerPoint;
            GermAreaManager.UpperPoint = manager.upperPoint;
            GermAreaManager.CenterPoint = manager.centerPoint;
            GermAreaManager.OuterPoint = manager.outerPoint;
        }
        else
        {
            Debug.LogError("GermAreaManager with tag 'pointManager' not found in the scene!");
            return;
        }

        // Initialize health
        currentHealth = maxHealth;

        // Assign a random color to the germ's material
        AssignRandomColor();

        // Set a random move speed within the min and max range
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        turnSpeed = Random.Range(5f, 8f);

        // Get the attached ParticleSystem component for the death effect
        deathEffect = GetComponent<ParticleSystem>();

        if (deathEffect != null)
        {
            var mainModule = deathEffect.main;
            mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            mainModule.duration = 1.5f;
        }

        // Calculate the cylinder bounds and set an initial curved path
        CalculateCylinderBounds();
        SetNewCurvedPath();
    }

    private void CalculateCylinderBounds()
    {
        cylinderRadius = Vector3.Distance(GermAreaManager.CenterPoint.position, GermAreaManager.OuterPoint.position);
        lowerBoundY = GermAreaManager.LowerPoint.position.y;
        upperBoundY = GermAreaManager.UpperPoint.position.y;
    }

    private void AssignRandomColor()
    {
        if (germRenderer != null)
        {
            Material randomColorMaterial = new Material(germRenderer.sharedMaterial);
            randomColorMaterial.color = new Color(
                Random.Range(0.4f, 1f),
                Random.Range(0.4f, 1f),
                Random.Range(0.4f, 1f)
            );

            germRenderer.material = randomColorMaterial;
        }
        else
        {
            Debug.LogWarning("Germ renderer is not assigned on " + gameObject.name);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Calculate the movement increment based on speed and curve length
        float moveIncrement = moveSpeed / curveLength;

        // Move along the curved path
        t += moveIncrement * Time.deltaTime;

        // Interpolate the position on the Bezier curve
        Vector3 nextPosition = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
        Vector3 direction = (nextPosition - transform.position).normalized;
        transform.position = nextPosition;

        // Rotate smoothly towards the path direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // If the curve is completed, set a new path
        if (t >= 1f)
        {
            SetNewCurvedPath();
        }
    }

    private void SetNewCurvedPath()
    {
        // Reset interpolation factor
        t = 0f;

        // Set start point as the germ's current position
        startPoint = transform.position;

        // Generate new random points within bounds for a smooth curve path
        controlPoint1 = GenerateRandomPointWithinCylinder();
        controlPoint2 = GenerateRandomPointWithinCylinder();
        endPoint = GenerateRandomPointWithinCylinder();

        // Calculate the length of the new curve for consistent movement speed
        curveLength = CalculateCurveLength(startPoint, controlPoint1, controlPoint2, endPoint);
    }

    private Vector3 GenerateRandomPointWithinCylinder()
    {
        // Generate a random angle and radius within the cylindrical bounds
        float randomAngle = Random.Range(0, 360) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(0, cylinderRadius);
        float randomY = Random.Range(lowerBoundY, upperBoundY);

        // Calculate the new position within the cylinder
        Vector3 offset = new Vector3(
            Mathf.Cos(randomAngle) * randomRadius,
            randomY - GermAreaManager.CenterPoint.position.y,
            Mathf.Sin(randomAngle) * randomRadius
        );

        return GermAreaManager.CenterPoint.position + offset;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Cubic Bezier curve calculation
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1 - t)^3 * p0
        point += 3 * uu * t * p1; // 3 * (1 - t)^2 * t * p1
        point += 3 * u * tt * p2; // 3 * (1 - t) * t^2 * p2
        point += ttt * p3; // t^3 * p3

        return point;
    }

    private float CalculateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int numSamples = 10)
    {
        // Approximate curve length by sampling points
        float length = 0f;
        Vector3 previousPoint = p0;

        for (int i = 1; i <= numSamples; i++)
        {
            float t = (float)i / numSamples;
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
            length += Vector3.Distance(previousPoint, point);
            previousPoint = point;
        }

        return length;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (deathEffect != null)
        {
            deathEffect.Stop();
            deathEffect.Play();
            Invoke(nameof(StopParticleEffect), 0.1f);
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(DestroyAfterDelay(0.5f));
        }
    }

    private void StopParticleEffect()
    {
        if (deathEffect != null)
        {
            deathEffect.Stop();
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }
}
