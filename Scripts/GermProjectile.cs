using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    public float speed = 10f;
    public float spinSpeed = 360f;  // degrees per second

    public void Initialize(Vector3 direction)
    {
        moveDirection = direction;
    }

    void Update()
    {
        // Move the projectile in the specified direction
        transform.position += moveDirection * speed * Time.deltaTime;

        // Rotate the projectile around its forward axis to create a spinning effect
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle collision with the player (e.g., apply damage)
            Destroy(gameObject); // Destroy the projectile upon hitting the player
        }
    }
}
