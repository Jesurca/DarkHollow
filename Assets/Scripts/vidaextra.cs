using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>();

            if (health == null)
            {
                return;
            }

            health.Heal(healAmount);

            Destroy(gameObject);
        }
    }
}
