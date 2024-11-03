using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Germ : MonoBehaviour
{
    public int maxHealth = 1;  // Start with 1 health point
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

    private ParticleSystem deathEffect;
    private bool isDead = false; // Flag to track if the germ is dead

    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Add this germ to the list
        allGerms.Add(this);

        // Assign a random color to the germ's material
        AssignRandomColor();

        // Randomly assign movement parameters
        moveSpeed = Random.Range(5f, 9f);
        turnSpeed = Random.Range(5f, 8f);
        neighborRadius = Random.Range(3f, 5f);
        avoidanceRadius = neighborRadius * 0.5f;

        // Decide whether this germ will follow the player
        isFollowingPlayer = Random.value < followPlayerChance;

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Get the attached ParticleSystem component for the death effect
        deathEffect = GetComponent<ParticleSystem>();

        // Ensure the particle system scales with the germ
        if (deathEffect != null)
        {
            var mainModule = deathEffect.main;
            mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
            mainModule.duration = 1.5f;  // Set the particle effect duration to 1.5 seconds
        }
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
        if (isDead) return; // Skip movement and behaviors if the germ is dead

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
        if (isDead) return; // Prevent further actions if germ is already dead

        currentHealth -= 1;

        // Play the particle effect once per hit, then stop it shortly after
        if (deathEffect != null)
        {
            deathEffect.Stop();  // Stop any ongoing playback to reset the particle effect
            deathEffect.Play();  // Play the effect once per hit
            Invoke(nameof(StopParticleEffect), 0.1f); // Stop the particle effect after 0.1 seconds
        }

        // Check if the germ's health has reached zero
        if (currentHealth <= 0)
        {
            isDead = true; // Mark as dead to prevent further updates
            StartCoroutine(DestroyAfterDelay(0.5f)); // Start the delayed destruction process
        }
    }

    // Method to stop the particle effect after a delay
    private void StopParticleEffect()
    {
        if (deathEffect != null)
        {
            deathEffect.Stop();
        }
    }


    private IEnumerator DestroyAfterDelay(float delay)
    {
        // Hide visual components by destroying child objects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Wait for the delay, allowing the particle effect to play
        yield return new WaitForSeconds(delay);

        // Remove the germ from the list and destroy the GameObject
        allGerms.Remove(this);
        Destroy(gameObject);
    }
}
