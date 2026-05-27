
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 20f;

    public float lifeTime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Golpeo: " + other.name);

        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);

            Destroy(gameObject);
        }
    }
}

