
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Tooltip("Cantidad de vida que pierde el jugador cuando este enemigo logra atacarlo.")]
    public int damage = 1;

    [Tooltip("Distancia maxima en metros para que el enemigo pueda hacer dano. Menor valor = debe estar mas pegado al jugador.")]
    public float attackRange = 0.42f;

    [Tooltip("Tiempo en segundos entre un golpe y el siguiente. Evita que quite toda la vida instantaneamente.")]
    public float attackCooldown = 1.5f;

    [Tooltip("Velocidad usada si el NavMesh falla o el enemigo no esta sobre una zona navegable.")]
    public float fallbackMoveSpeed = 1.8f;

    [Tooltip("Cada cuantos segundos recalcula el destino hacia el jugador cuando usa NavMesh.")]
    public float repathInterval = 0.2f;

    [Tooltip("Distancia en metros que el enemigo retrocede despues de hacer dano.")]
    public float knockbackDistance = 1.4f;

    [Tooltip("Duracion en segundos del retroceso despues de hacer dano.")]
    public float knockbackDuration = 0.25f;

    Transform player;
    PlayerHealth playerHealth;
    NavMeshAgent agent;
    Invisibility invisibility;
    float nextDamageTime;
    float nextRepathTime;
    float nextWanderTime;
    bool isKnockedBack;
    Vector3 wanderTarget;

    static float frozenUntilTime;

    public static void FreezeAll(float duration)
    {
        frozenUntilTime = Mathf.Max(frozenUntilTime, Time.time + duration);
        Debug.Log("Reloj activado. Enemigos congelados por " + duration + " segundos.");
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            return;
        }

        if (Time.time < frozenUntilTime)
        {
            StopMovement();
            TryDamagePlayer();
            return;
        }

        if (invisibility != null &&
            invisibility.IsInvisible())
        {
            WanderWhilePlayerIsInvisible();
            return;
        }

        MoveToPlayer();
        TryDamagePlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;
        playerHealth = playerObject.GetComponent<PlayerHealth>();
        invisibility = playerObject.GetComponent<Invisibility>();
    }

    void MoveToPlayer()
    {
        if (isKnockedBack)
        {
            return;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;

            if (Time.time >= nextRepathTime)
            {
                agent.SetDestination(player.position);
                nextRepathTime = Time.time + repathInterval;
            }

            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 movement =
            direction.normalized *
            fallbackMoveSpeed *
            Time.deltaTime;

        transform.position += movement;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void WanderWhilePlayerIsInvisible()
    {
        if (isKnockedBack)
        {
            return;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (!agent.hasPath || Time.time >= nextWanderTime)
            {
                Vector3 randomDirection =
                    Random.insideUnitSphere * 4f + transform.position;

                if (NavMesh.SamplePosition(
                    randomDirection,
                    out NavMeshHit hit,
                    4f,
                    NavMesh.AllAreas
                ))
                {
                    agent.isStopped = false;
                    agent.SetDestination(hit.position);
                }

                nextWanderTime = Time.time + Random.Range(1.5f, 3f);
            }

            return;
        }

        if (Time.time >= nextWanderTime)
        {
            Vector2 random = Random.insideUnitCircle.normalized;
            wanderTarget = transform.position + new Vector3(random.x, 0f, random.y) * 3f;
            nextWanderTime = Time.time + Random.Range(1.5f, 3f);
        }

        Vector3 direction = wanderTarget - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.05f)
        {
            return;
        }

        transform.position +=
            direction.normalized *
            fallbackMoveSpeed *
            0.6f *
            Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    void StopMovement()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    void TryDamagePlayer()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (player == null || playerHealth == null)
        {
            return;
        }

        if (invisibility != null && invisibility.IsInvisible())
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        Vector3 enemyAttackPoint = transform.position + Vector3.up * 0.5f;
        Vector3 playerAttackPoint = player.position + Vector3.up * 0.5f;
        Vector3 delta = playerAttackPoint - enemyAttackPoint;
        delta.y = 0f;

        if (delta.sqrMagnitude > attackRange * attackRange)
        {
            return;
        }

        playerHealth.TakeDamage(damage);
        nextDamageTime = Time.time + attackCooldown;

        if (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(KnockbackFromPlayer());
        }
    }

    System.Collections.IEnumerator KnockbackFromPlayer()
    {
        if (player == null)
        {
            yield break;
        }

        isKnockedBack = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away.sqrMagnitude < 0.01f)
        {
            away = -transform.forward;
        }

        away.Normalize();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + away * knockbackDistance;
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.Warp(transform.position);
            agent.isStopped = false;
        }

        isKnockedBack = false;
    }
}

