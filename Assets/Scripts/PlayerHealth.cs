using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Cantidad de vidas con las que empieza el jugador.")]
    public int maxLives = 6;
    public int currentLives;
    public Color damageFlashColor = new Color(1f, 0.12f, 0.12f, 1f);
    public float damageFlashDuration = 0.15f;

    bool isDead;
    readonly List<Material> runtimeMaterials = new List<Material>();
    readonly List<MaterialColorState> originalMaterialColors = new List<MaterialColorState>();
    Coroutine damageFlashRoutine;

    public event Action<int, int> LivesChanged;

    void Start()
    {
        CacheRenderers();
        EnsureHud();

        currentLives = maxLives;
        LivesChanged?.Invoke(currentLives, maxLives);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentLives -= damage;
        currentLives = Mathf.Max(currentLives, 0);

        LivesChanged?.Invoke(currentLives, maxLives);
        ShowDamageFlash();

        Debug.Log("Harry recibio dano. Vidas: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentLives += amount;

        if (currentLives > maxLives)
        {
            currentLives = maxLives;
        }

        LivesChanged?.Invoke(currentLives, maxLives);
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Harry murio.");
    }

    void CacheRenderers()
    {
        runtimeMaterials.Clear();
        originalMaterialColors.Clear();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in renderer.materials)
            {
                runtimeMaterials.Add(material);
                originalMaterialColors.Add(new MaterialColorState(material));
            }
        }
    }

    void ShowDamageFlash()
    {
        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
            RestoreOriginalColors();
        }

        damageFlashRoutine = StartCoroutine(DamageFlash());
    }

    IEnumerator DamageFlash()
    {
        foreach (Material material in runtimeMaterials)
        {
            SetMaterialColor(material, damageFlashColor);
        }

        yield return new WaitForSeconds(damageFlashDuration);

        RestoreOriginalColors();
        damageFlashRoutine = null;
    }

    void RestoreOriginalColors()
    {
        foreach (MaterialColorState colorState in originalMaterialColors)
        {
            colorState.Restore();
        }
    }

    void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    void EnsureHud()
    {
        if (FindAnyObjectByType<PlayerHealthHud>() != null)
        {
            return;
        }

        GameObject hudObject = new GameObject("Sistema_HUD_Vidas");
        PlayerHealthHud hud = hudObject.AddComponent<PlayerHealthHud>();
        hud.playerHealth = this;
    }

    void EnsurePotionSpawner()
    {
        if (FindAnyObjectByType<HealthPotionSpawner>() != null)
        {
            return;
        }

        GameObject spawnerObject = new GameObject("Sistema_Spawner_Items");
        HealthPotionSpawner spawner =
            spawnerObject.AddComponent<HealthPotionSpawner>();
        spawner.playerHealth = this;
    }

    class MaterialColorState
    {
        readonly Material material;
        readonly bool hasBaseColor;
        readonly bool hasColor;
        readonly Color baseColor;
        readonly Color color;

        public MaterialColorState(Material material)
        {
            this.material = material;

            if (material == null)
            {
                return;
            }

            hasBaseColor = material.HasProperty("_BaseColor");
            hasColor = material.HasProperty("_Color");

            if (hasBaseColor)
            {
                baseColor = material.GetColor("_BaseColor");
            }

            if (hasColor)
            {
                color = material.GetColor("_Color");
            }
        }

        public void Restore()
        {
            if (material == null)
            {
                return;
            }

            if (hasBaseColor)
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (hasColor)
            {
                material.SetColor("_Color", color);
            }
        }
    }
}

public class PlayerHealthHud : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Color fullLifeColor = new Color(0.74f, 0.12f, 0.18f, 1f);
    public Color emptyLifeColor = new Color(0.18f, 0.16f, 0.2f, 0.95f);

    TMP_Text livesText;

    void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        CreateHud();

        if (playerHealth != null)
        {
            playerHealth.LivesChanged += UpdateLives;
            UpdateLives(playerHealth.currentLives, playerHealth.maxLives);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.LivesChanged -= UpdateLives;
        }
    }

    void CreateHud()
    {
        GameObject canvasObject = new GameObject("HUD_Vidas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("Panel_Vidas");
        panelObject.transform.SetParent(canvasObject.transform, false);

        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.03f, 0.025f, 0.04f, 0.72f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(24f, -22f);
        panelRect.sizeDelta = new Vector2(300f, 68f);

        GameObject titleObject = new GameObject("Texto_Vidas");
        titleObject.transform.SetParent(panelObject.transform, false);

        TextMeshProUGUI title = titleObject.AddComponent<TextMeshProUGUI>();
        title.font = Resources.Load<TMP_FontAsset>("Fonts/Harry SDF");
        title.fontSize = 40f;
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.color = new Color(0.92f, 0.86f, 0.68f, 1f);
        title.text = "Vidas";

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.anchoredPosition = new Vector2(18f, 0f);
        titleRect.sizeDelta = new Vector2(104f, 0f);

        GameObject livesObject = new GameObject("Marcadores_Vidas");
        livesObject.transform.SetParent(panelObject.transform, false);

        livesText = livesObject.AddComponent<TextMeshProUGUI>();
        livesText.font = Resources.Load<TMP_FontAsset>("Fonts/Harry SDF");
        livesText.fontSize = 38f;
        livesText.alignment = TextAlignmentOptions.MidlineLeft;
        livesText.richText = true;

        RectTransform livesRect = livesObject.GetComponent<RectTransform>();
        livesRect.anchorMin = new Vector2(0f, 0f);
        livesRect.anchorMax = new Vector2(1f, 1f);
        livesRect.pivot = new Vector2(0f, 0.5f);
        livesRect.offsetMin = new Vector2(126f, 0f);
        livesRect.offsetMax = new Vector2(-12f, 0f);
    }

    void UpdateLives(int currentLives, int maxLives)
    {
        if (livesText == null)
        {
            return;
        }

        string fullColor = ColorUtility.ToHtmlStringRGB(fullLifeColor);
        string emptyColor = ColorUtility.ToHtmlStringRGB(emptyLifeColor);
        string text = string.Empty;

        for (int i = 0; i < maxLives; i++)
        {
            string color = i < currentLives ? fullColor : emptyColor;
            text += "<color=#" + color + ">O</color>";

            if (i < maxLives - 1)
            {
                text += " ";
            }
        }

        livesText.text = text;
    }
}
