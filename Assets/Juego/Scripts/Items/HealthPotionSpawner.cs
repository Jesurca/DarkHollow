using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HealthPotionSpawner : MonoBehaviour
{
    [Tooltip("Prefab u objeto plantilla de la pocion. Si se deja vacio, intenta usar Pickup_VidaExtra de la escena.")]
    public GameObject pocionPrefab;

    [Tooltip("Prefab u objeto plantilla del reloj. Si se deja vacio, intenta usar Reloj de la escena.")]
    public GameObject relojPrefab;

    [Tooltip("Prefab u objeto plantilla del libro. Si se deja vacio, intenta usar Libro de la escena.")]
    public GameObject libroPrefab;

    [HideInInspector]
    public ItemSpawnZone[] spawnZones;

    [Tooltip("Puntos donde pueden aparecer pociones. Si esta vacio, usa los puntos de spawn de enemigos.")]
    public Transform[] spawnPoints;

    [Tooltip("Jugador usado para revisar cuanta vida tiene.")]
    public PlayerHealth playerHealth;

    [Tooltip("Tiempo en segundos antes del primer intento de generar pocion.")]
    public float firstSpawnDelay = 6f;

    [Tooltip("Tiempo en segundos entre apariciones de items.")]
    public float spawnCheckInterval = 12f;

    [Tooltip("Maximo de pociones simultaneas en el mapa.")]
    public int maxItemsInScene = 2;

    [Tooltip("Si esta activo, intenta crear una primera pocion sin depender de probabilidad.")]
    public bool forceFirstItem = true;

    [Tooltip("Probabilidad base de crear un item. En 1 siempre intenta crear.")]
    [Range(0f, 1f)]
    public float baseSpawnChance = 0.75f;

    [Tooltip("Probabilidad adicional cuando el jugador tiene poca vida.")]
    [Range(0f, 1f)]
    public float lowHealthBonusChance = 0.4f;

    [Tooltip("Si el jugador tiene esta cantidad de vidas o menos, aumenta la probabilidad de pocion.")]
    public int lowHealthThreshold = 3;

    [Tooltip("Peso base de la pocion. Mayor valor = sale mas seguido.")]
    public float potionWeight = 0.9f;

    [Tooltip("Peso base del reloj. Mayor valor = sale mas seguido.")]
    public float clockWeight = 0.7f;

    [Tooltip("Peso base del libro. Mayor valor = sale mas seguido.")]
    public float bookWeight = 0.7f;

    [Tooltip("Cantidad de enemigos desde la que el sistema aumenta ayudas fuertes.")]
    public int highEnemyPressureThreshold = 8;

    [Tooltip("Bonus de peso para reloj/libro cuando hay muchos enemigos.")]
    public float highEnemyPressureBonus = 0.8f;

    [Tooltip("Bonus de peso para pocion cuando el jugador tiene poca vida.")]
    public float lowHealthPotionWeightBonus = 1f;

    [Tooltip("Radio para ajustar el punto de aparicion a una zona valida del NavMesh.")]
    public float navMeshSearchRadius = 3f;

    [Tooltip("Si no hay zonas configuradas, intenta generar items cerca del jugador.")]
    public float fallbackRadiusAroundPlayer = 4f;

    float nextSpawnCheckTime;
    bool spawnedFirstItem;
    int lastSpawnPointIndex = -1;

    void Start()
    {
        FindReferences();
        nextSpawnCheckTime = Time.time + firstSpawnDelay;
    }

    void Update()
    {
        if (Time.time < nextSpawnCheckTime)
        {
            return;
        }

        nextSpawnCheckTime = Time.time + spawnCheckInterval;
        TrySpawnPotion();
    }

    void FindReferences()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        pocionPrefab = FindPrefabAssetIfNeeded(
            pocionPrefab,
            "Poción",
            "Pickup_VidaExtra",
            "Pocion",
            "Potion"
        );
        relojPrefab = FindPrefabAssetIfNeeded(relojPrefab, "Reloj", "Clock");
        libroPrefab = FindPrefabAssetIfNeeded(libroPrefab, "Libro", "Book");

        pocionPrefab = FindTemplateIfNeeded(pocionPrefab, "Pickup_VidaExtra");
        relojPrefab = FindTemplateIfNeeded(relojPrefab, "Reloj");
        libroPrefab = FindTemplateIfNeeded(libroPrefab, "Libro");

        HideSceneTemplate(pocionPrefab);
        HideSceneTemplate(relojPrefab);
        HideSceneTemplate(libroPrefab);

        CreateFallbackTemplatesIfNeeded();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] enemySpawnObjects =
                GameObject.FindGameObjectsWithTag("Untagged");

            var points = new System.Collections.Generic.List<Transform>();

            foreach (GameObject candidate in enemySpawnObjects)
            {
                if (candidate.name.StartsWith("Spawn_Enemigo"))
                {
                    points.Add(candidate.transform);
                }
            }

            spawnPoints = points.ToArray();
        }
    }

    GameObject FindTemplateIfNeeded(GameObject currentTemplate, string objectName)
    {
        if (currentTemplate != null)
        {
            return currentTemplate;
        }

        GameObject activeObject = GameObject.Find(objectName);

        if (activeObject != null)
        {
            return activeObject;
        }

        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate.name == objectName &&
                candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        Debug.LogWarning("No se encontro plantilla de item: " + objectName);
        return null;
    }

    GameObject FindPrefabAssetIfNeeded(
        GameObject currentTemplate,
        params string[] possibleNames
    )
    {
        if (currentTemplate != null)
        {
            return currentTemplate;
        }

        foreach (string possibleName in possibleNames)
        {
            GameObject resourcePrefab =
                Resources.Load<GameObject>("Items/" + possibleName);

            if (resourcePrefab != null)
            {
                return resourcePrefab;
            }

            resourcePrefab = Resources.Load<GameObject>(possibleName);

            if (resourcePrefab != null)
            {
                return resourcePrefab;
            }
        }

#if UNITY_EDITOR
        foreach (string possibleName in possibleNames)
        {
            string[] guids = AssetDatabase.FindAssets(possibleName + " t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    Debug.Log("Prefab de item encontrado: " + path);
                    return prefab;
                }
            }
        }
#endif

        return null;
    }

    void TrySpawnPotion()
    {
        GameObject selectedTemplate = ChooseItemTemplate();

        if (selectedTemplate == null)
        {
            return;
        }

        if (CountActiveItems() >= maxItemsInScene)
        {
            return;
        }

        float chance = baseSpawnChance;

        if (forceFirstItem && !spawnedFirstItem)
        {
            chance = 1f;
        }

        if (playerHealth != null &&
            playerHealth.currentLives <= lowHealthThreshold)
        {
            chance += lowHealthBonusChance;
        }

        if (Random.value > Mathf.Clamp01(chance))
        {
            return;
        }

        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
        {
            return;
        }

        GameObject item = Instantiate(
            selectedTemplate,
            spawnPosition,
            Quaternion.identity
        );

        ConfigureSpawnedItem(item, GetItemType(selectedTemplate));
        item.name = GetSpawnedItemName(selectedTemplate);
        item.SetActive(true);
        spawnedFirstItem = true;
        Debug.Log("Item generado: " + item.name + " en " + spawnPosition);
    }

    GameObject ChooseItemTemplate()
    {
        float potion = pocionPrefab != null ? potionWeight : 0f;
        float clock = relojPrefab != null ? clockWeight : 0f;
        float book = libroPrefab != null ? bookWeight : 0f;

        if (playerHealth != null &&
            playerHealth.currentLives <= lowHealthThreshold)
        {
            potion += lowHealthPotionWeightBonus;
        }

        int activeEnemies =
            FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude).Length;

        if (activeEnemies >= highEnemyPressureThreshold)
        {
            clock += highEnemyPressureBonus;
            book += highEnemyPressureBonus;
        }

        float totalWeight = potion + clock + book;

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.value * totalWeight;

        if (roll < potion)
        {
            return pocionPrefab;
        }

        roll -= potion;

        if (roll < clock)
        {
            return relojPrefab;
        }

        return libroPrefab;
    }

    void HideSceneTemplate(GameObject template)
    {
        if (template == null)
        {
            return;
        }

        if (template.scene.IsValid())
        {
            template.SetActive(false);
        }
    }

    void ConfigureSpawnedItem(GameObject item, ItemPickupType itemType)
    {
        ItemPickupEffect pickup = item.GetComponent<ItemPickupEffect>();

        if (pickup == null)
        {
            pickup = item.AddComponent<ItemPickupEffect>();
        }

        pickup.itemType = itemType;

        if (itemType == ItemPickupType.HealthPotion)
        {
            HealthPickup oldPickup = item.GetComponent<HealthPickup>();

            if (oldPickup != null)
            {
                pickup.healAmount = oldPickup.healAmount;
                oldPickup.enabled = false;
            }
        }

        Collider collider = item.GetComponent<Collider>();

        if (collider == null)
        {
            collider = item.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;

        if (item.GetComponent<ItemVisualEffect>() == null)
        {
            item.AddComponent<ItemVisualEffect>();
        }
    }

    void CreateFallbackTemplatesIfNeeded()
    {
        if (pocionPrefab == null)
        {
            pocionPrefab = CreateFallbackTemplate(
                "Pickup_VidaExtra_Runtime",
                ItemPickupType.HealthPotion,
                Color.red
            );
        }

        if (relojPrefab == null)
        {
            relojPrefab = CreateFallbackTemplate(
                "Reloj_Runtime",
                ItemPickupType.Clock,
                Color.yellow
            );
        }

        if (libroPrefab == null)
        {
            libroPrefab = CreateFallbackTemplate(
                "Libro_Runtime",
                ItemPickupType.Book,
                Color.cyan
            );
        }
    }

    GameObject CreateFallbackTemplate(
        string objectName,
        ItemPickupType itemType,
        Color color
    )
    {
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
        template.name = objectName;
        template.transform.localScale = Vector3.one * 0.45f;

        Renderer renderer = template.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material.color = color;
        }

        ItemPickupEffect pickup = template.AddComponent<ItemPickupEffect>();
        pickup.itemType = itemType;

        Collider collider = template.GetComponent<Collider>();
        collider.isTrigger = true;

        template.AddComponent<ItemVisualEffect>();
        template.SetActive(false);
        return template;
    }

    string GetSpawnedItemName(GameObject template)
    {
        ItemPickupType itemType = GetItemType(template);

        if (itemType == ItemPickupType.HealthPotion)
        {
            return "Pocion_Vida";
        }

        if (itemType == ItemPickupType.Clock)
        {
            return "Reloj_Tiempo";
        }

        return "Libro_Invisibilidad";
    }

    ItemPickupType GetItemType(GameObject template)
    {
        if (template == relojPrefab)
        {
            return ItemPickupType.Clock;
        }

        if (template == libroPrefab)
        {
            return ItemPickupType.Book;
        }

        return ItemPickupType.HealthPotion;
    }

    bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return TryGetFallbackPositionAroundPlayer(out spawnPosition);
        }

        Transform spawnPoint = spawnPoints[GetNextSpawnPointIndex()];

        spawnPosition = spawnPoint.position;

        if (NavMesh.SamplePosition(
            spawnPosition,
            out NavMeshHit hit,
            navMeshSearchRadius,
            NavMesh.AllAreas
        ))
        {
            spawnPosition = hit.position;
        }

        spawnPosition += Vector3.up * 0.35f;
        return true;
    }

    int GetNextSpawnPointIndex()
    {
        if (spawnPoints.Length <= 1)
        {
            lastSpawnPointIndex = 0;
            return 0;
        }

        int index = Random.Range(0, spawnPoints.Length);

        for (int i = 0; i < 8 && index == lastSpawnPointIndex; i++)
        {
            index = Random.Range(0, spawnPoints.Length);
        }

        lastSpawnPointIndex = index;
        return index;
    }

    bool TryGetFallbackPositionAroundPlayer(out Vector3 spawnPosition)
    {
        if (playerHealth == null)
        {
            spawnPosition = Vector3.zero;
            return false;
        }

        for (int i = 0; i < 12; i++)
        {
            Vector2 random = Random.insideUnitCircle * fallbackRadiusAroundPlayer;
            Vector3 candidate =
                playerHealth.transform.position +
                new Vector3(random.x, 0f, random.y);

            if (NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSearchRadius,
                NavMesh.AllAreas
            ))
            {
                spawnPosition = hit.position + Vector3.up * 0.35f;
                return true;
            }
        }

        spawnPosition = playerHealth.transform.position + Vector3.up * 0.35f;
        return true;
    }

    int CountActiveItems()
    {
        int count = 0;

        foreach (ItemPickupEffect pickup in FindObjectsByType<ItemPickupEffect>(
            FindObjectsInactive.Exclude
        ))
        {
            if (pickup.gameObject != pocionPrefab &&
                pickup.gameObject != relojPrefab &&
                pickup.gameObject != libroPrefab)
            {
                count++;
            }
        }

        return count;
    }
}
