
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private Transform player;

    private NavMeshAgent agent;

    private Invisibility invisibility;

    public int damage = 1;

    private bool canDamage = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            invisibility =
                player.GetComponent<Invisibility>();
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (invisibility != null &&
            invisibility.IsInvisible())
        {
            agent.ResetPath();

            return;
        }

        agent.SetDestination(player.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage)
            return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health =
                other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);

                StartCoroutine(DamageCooldown());
            }
        }
    }

    System.Collections.IEnumerator DamageCooldown()
    {
        canDamage = false;

        yield return new WaitForSeconds(1f);

        canDamage = true;
    }
}

