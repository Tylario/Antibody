using UnityEngine;
using System.Collections.Generic;

public class Germ : MonoBehaviour
{
    // Health parameters
    public int maxHealth = 3;
    private int currentHealth;

    // Movement parameters
    private float moveSpeed;
    private float turnSpeed;
    private float neighborRadius;
    private float avoidanceRadius;

    // Boid behavior weights
    private float alignmentWeight = 1f;
    private float cohesionWeight = 1f;
    private float separationWeight = 1.5f;

    // Reference to all germs for boid behaviors
    private static List<Germ> allGerms = new List<Germ>();

    // Random chance to follow the player
    public float followPlayerChance = 0.3f;
    private bool isFollowingPlayer = false;

    // Reference to the player
    private Transform playerTransform;

    // Renderer for the germ's visual appearance
    public Renderer germRenderer;

    // Raycasting for obstacle avoidance
    public LayerMask obstacleLayer;
    private float raycastDistance = 2f;

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Add this germ to the list
        allGerms.Add(this);

        // Assign a random color to the germ's material
        AssignRandomColor();

        // Randomly assign movement parameters
        moveSpeed = Random.Range(1f, 3f);
        turnSpeed = Random.Range(2f, 5f);
        neighborRadius = Random.Range(3f, 5f);
        avoidanceRadius = neighborRadius * 0.5f;

        // Decide whether this germ will follow the player
        isFollowingPlayer = Random.value < followPlayerChance;

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void AssignRandomColor()
    {
        if (germRenderer != null)
        {
            // Create a new material instance with a random color
            Material randomColorMaterial = new Material(germRenderer.sharedMaterial); // Using sharedMaterial to avoid creating multiple instances unnecessarily
            randomColorMaterial.color = new Color(
                Random.Range(0.4f, 1f),
                Random.Range(0.4f, 1f),
                Random.Range(0.4f, 1f)
            );

            // Assign the newly created material to the renderer
            germRenderer.material = randomColorMaterial;
        }
        else
        {
            Debug.LogWarning("Germ renderer is not assigned on " + gameObject.name);
        }
    }

    void Update()
    {
        Vector3 acceleration = Vector3.zero;

        // Boid behaviors
        acceleration += Alignment() * alignmentWeight;
        acceleration += Cohesion() * cohesionWeight;
        acceleration += Separation() * separationWeight;

        // Follow or orbit the player if this germ is assigned to
        if (isFollowingPlayer && playerTransform != null)
        {
            acceleration += FollowPlayer();
        }

        // Obstacle avoidance
        acceleration += ObstacleAvoidance();

        // Move in the forward direction
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // Rotate towards the calculated acceleration
        if (acceleration != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(acceleration.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
        }
    }

    private Vector3 Alignment()
    {
        Vector3 alignmentMove = Vector3.zero;
        int count = 0;

        foreach (Germ germ in allGerms)
        {
            if (germ != this)
            {
                float distance = Vector3.Distance(transform.position, germ.transform.position);
                if (distance < neighborRadius)
                {
                    alignmentMove += germ.transform.forward;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            alignmentMove /= count;
            alignmentMove = alignmentMove.normalized;
        }

        return alignmentMove;
    }

    private Vector3 Cohesion()
    {
        Vector3 cohesionMove = Vector3.zero;
        int count = 0;

        foreach (Germ germ in allGerms)
        {
            if (germ != this)
            {
                float distance = Vector3.Distance(transform.position, germ.transform.position);
                if (distance < neighborRadius)
                {
                    cohesionMove += germ.transform.position;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            cohesionMove /= count;
            cohesionMove = (cohesionMove - transform.position).normalized;
        }

        return cohesionMove;
    }

    private Vector3 Separation()
    {
        Vector3 separationMove = Vector3.zero;
        int count = 0;

        foreach (Germ germ in allGerms)
        {
            if (germ != this)
            {
                float distance = Vector3.Distance(transform.position, germ.transform.position);
                if (distance < avoidanceRadius)
                {
                    separationMove += (transform.position - germ.transform.position) / distance;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separationMove /= count;
            separationMove = separationMove.normalized;
        }

        return separationMove;
    }

    private Vector3 FollowPlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        // Optionally, make the germs orbit around the player
        Vector3 orbitDirection = Vector3.Cross(Vector3.up, directionToPlayer).normalized;

        // Mix between following directly and orbiting
        return (directionToPlayer + orbitDirection * 0.5f).normalized;
    }

    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance, obstacleLayer))
        {
            // Calculate a direction to avoid the obstacle
            Vector3 avoidanceDir = Vector3.Reflect(transform.forward, hit.normal);
            return avoidanceDir.normalized * 2f; // Multiply to give higher priority
        }

        return Vector3.zero;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            // Optionally, play a death effect here (e.g., particle system, sound)
            DestroyGerm();
        }
    }

    private void DestroyGerm()
    {
        // Remove this germ from the list
        allGerms.Remove(this);

        // Destroy the germ GameObject
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Ensure the germ is removed from the list if destroyed by other means
        allGerms.Remove(this);
    }
}
