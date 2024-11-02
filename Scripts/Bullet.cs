using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // Speed of the bullet
    public float lifetime = 2f; // Time before the bullet is destroyed
    public int damage = 10; // Damage dealt by the bullet (optional)


    private void Start()
    {
        // Destroy the bullet after a certain time to avoid infinite lifespan
        Destroy(gameObject, lifetime);

    }

    private void Update()
    {
        // Move the bullet forward based on its own forward direction
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO
        
        // Destroy the bullet
        Destroy(gameObject);
    }
}
