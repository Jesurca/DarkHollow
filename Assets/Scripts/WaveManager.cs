
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class WaveManager : MonoBehaviour
{
    [Tooltip("Prefab del enemigo que se va a crear durante las oleadas.")]
    public GameObject enemyPrefab;

    [Tooltip("Puntos posibles desde donde aparecen los enemigos. Se elige uno al azar por cada enemigo.")]
    public Transform[] spawnPoints;

    [Tooltip("Oleada actual. Normalmente empieza en 1 y aumenta automaticamente.")]
    public int currentWave = 1;

    [Tooltip("Tiempo en segundos antes de que salga el primer enemigo al iniciar la escena. Ejemplo: 0.5 = medio segundo.")]
    public float initialDelay = 1f;

    [Tooltip("Tiempo en segundos entre el final de una oleada y el inicio de la siguiente.")]
    public float timeBetweenWaves = 12f;

    [Tooltip("Tiempo en segundos entre un enemigo y el siguiente dentro de la misma oleada. Menor valor = salen mas seguido.")]
    public float spawnInterval = 0.45f;

    [Tooltip("Cantidad de enemigos en la primera oleada.")]
    public int enemiesFirstWave = 5;

    [Tooltip("Cantidad adicional de enemigos que se agrega por cada nueva oleada.")]
    public int enemiesAddedPerWave = 5;

    [Tooltip("No se usa actualmente. Se dejo desactivado porque instanciar enemigos bajo el suelo rompe el NavMesh.")]
    public float riseDepth = 6f;

    [Tooltip("No se usa actualmente. Se dejo desactivado porque instanciar enemigos bajo el suelo rompe el NavMesh.")]
    public float riseSpeed = 4f;

    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            yield return StartCoroutine(SpawnWave());

            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
        }
    }

    IEnumerator SpawnWave()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveManager no tiene enemyPrefab o spawnPoints configurados.");
            yield break;
        }

        int enemyCount =
            enemiesFirstWave +
            (currentWave - 1) * enemiesAddedPerWave;

        for (int i = 0; i < enemyCount; i++)
        {
            Transform randomSpawn =
                spawnPoints[
                    Random.Range(0, spawnPoints.Length)
                ];

            Vector3 spawnPos = randomSpawn.position;

            if (NavMesh.SamplePosition(
                randomSpawn.position,
                out NavMeshHit spawnHit,
                3f,
                NavMesh.AllAreas
            ))
            {
                spawnPos = spawnHit.position;
            }

            GameObject enemy =
                Instantiate(
                    enemyPrefab,
                    spawnPos,
                    Quaternion.identity
                );

            yield return StartCoroutine(PrepareEnemy(enemy, spawnPos));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator PrepareEnemy(GameObject enemy, Vector3 spawnPos)
    {
        if (enemy == null)
        {
            yield break;
        }

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        if (agent != null &&
            NavMesh.SamplePosition(
                spawnPos,
                out NavMeshHit hit,
                3f,
                NavMesh.AllAreas
            ))
        {
            agent.enabled = true;
            agent.Warp(hit.position);
        }

        yield return new WaitForSeconds(0.15f);

        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }
    }
}

