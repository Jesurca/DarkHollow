using UnityEngine;
using UnityEngine.AI;

public class ItemSpawnZone : MonoBehaviour
{
    [Tooltip("Si esta activo, ajusta el punto elegido a una zona valida del NavMesh.")]
    public bool useNavMesh = true;

    [Tooltip("Radio para buscar NavMesh alrededor del punto aleatorio.")]
    public float navMeshSearchRadius = 3f;

    [Tooltip("Altura extra para que el item no aparezca hundido en el piso.")]
    public float surfaceOffset = 0.35f;

    Collider zoneCollider;

    void Awake()
    {
        CacheCollider();
    }

    void Reset()
    {
        Collider collider = GetComponent<Collider>();

        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;
    }

    public bool TryGetSpawnPosition(out Vector3 position)
    {
        CacheCollider();

        if (zoneCollider == null)
        {
            position = transform.position;
            return false;
        }

        Bounds bounds = zoneCollider.bounds;

        for (int i = 0; i < 10; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (!useNavMesh)
            {
                position = candidate + Vector3.up * surfaceOffset;
                return true;
            }

            if (NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSearchRadius,
                NavMesh.AllAreas
            ))
            {
                position = hit.position + Vector3.up * surfaceOffset;
                return true;
            }
        }

        position = bounds.center + Vector3.up * surfaceOffset;
        return true;
    }

    void CacheCollider()
    {
        if (zoneCollider == null)
        {
            zoneCollider = GetComponent<Collider>();
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider collider = GetComponent<Collider>();

        if (collider == null)
        {
            return;
        }

        Bounds bounds = collider.bounds;
        Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.25f);
        Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = new Color(0.25f, 1f, 0.35f, 0.9f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
