using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;      // Speed of the bullet
    public float lifetime = 2f;    // Time before the bullet is destroyed
    public int damage = 1;         // Damage dealt by the bullet
    public float detectionRange = 1f; // Distance ahead to check for germs

    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Raycast forward to detect germs
        DetectAndDamageGerms();
    }

    private void DetectAndDamageGerms()
    {
        RaycastHit hit;
        // Perform a raycast in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange))
        {
            // Check if the hit object has the tag "germ"
            if (hit.collider.CompareTag("germ"))
            {
                Germ germ = hit.collider.GetComponent<Germ>();
                if (germ != null)
                {
                    germ.TakeDamage(damage);

                    // Optionally, you can prevent multiple hits on the same germ by adding it to a list
                    // or adding a cooldown before it can be hit again.
                }
            }
        }
    }
}
