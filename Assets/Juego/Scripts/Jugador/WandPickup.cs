using UnityEngine;

public class WandPickup : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerAttack playerAttack = other.GetComponent<PlayerAttack>();

        if (playerAttack == null)
        {
            return;
        }

        playerAttack.EnableShooting();
        Destroy(gameObject);
    }
}
