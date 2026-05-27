
using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Transform[] spawnPoints;

    public int currentWave = 1;

    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            SpawnWave();

            yield return new WaitForSeconds(30f);

            currentWave++;
        }
    }

    void SpawnWave()
    {
        int enemyCount = currentWave * 5;

        for (int i = 0; i < enemyCount; i++)
        {
            Transform randomSpawn =
                spawnPoints[
                    Random.Range(0, spawnPoints.Length)
                ];

            Vector3 spawnPos = randomSpawn.position;

            spawnPos.y -= 10f;

            GameObject enemy =
                Instantiate(
                    enemyPrefab,
                    spawnPos,
                    Quaternion.identity
                );

            StartCoroutine(RiseEnemy(enemy));
        }
    }

    IEnumerator RiseEnemy(GameObject enemy)
    {
        Vector3 targetPos =
            new Vector3(
                enemy.transform.position.x,
                0,
                enemy.transform.position.z
            );

        while (
            Vector3.Distance(
                enemy.transform.position,
                targetPos
            ) > 0.1f
        )
        {
            enemy.transform.position =
                Vector3.MoveTowards(
                    enemy.transform.position,
                    targetPos,
                    2f * Time.deltaTime
                );

            yield return null;
        }
    }
}

