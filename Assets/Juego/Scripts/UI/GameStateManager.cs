using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    [Tooltip("Sistema que controla las oleadas de enemigos.")]
    [FormerlySerializedAs("waveManager")]
    public WaveManager sistemaOleadas;

    [Tooltip("Vida del jugador usada para detectar derrota.")]
    [FormerlySerializedAs("playerHealth")]
    public PlayerHealth vidaJugador;

    [Tooltip("Oleada que debe completarse para ganar. Ejemplo: 5 = ganar al terminar la oleada 5 y matar los enemigos restantes.")]
    [FormerlySerializedAs("waveToWin")]
    public int oleadaParaGanar = 5;

    [Tooltip("Texto usado solo para logs cuando el jugador gana.")]
    [FormerlySerializedAs("victoryMessage")]
    public string textoVictoria = "Victoria";

    [Tooltip("Texto usado solo para logs cuando el jugador pierde.")]
    [FormerlySerializedAs("defeatMessage")]
    public string textoDerrota = "Derrota";

    [Tooltip("Nombre o ruta de la escena de menu.")]
    public string nombreEscenaMenu = "Assets/Juego/Escenas/Menu.unity";

    [Header("Pantallas finales editables")]
    [Tooltip("Pantalla completa que aparece cuando el jugador gana. Debe estar en la escena e iniciar desactivada.")]
    public GameObject pantallaVictoria;

    [Tooltip("Pantalla completa que aparece cuando el jugador pierde. Debe estar en la escena e iniciar desactivada.")]
    public GameObject pantallaDerrota;

    [Tooltip("Boton Reintentar de la pantalla de victoria.")]
    public Button botonVictoriaReintentar;

    [Tooltip("Boton Menu de la pantalla de victoria.")]
    public Button botonVictoriaMenu;

    [Tooltip("Boton Reintentar de la pantalla de derrota.")]
    public Button botonDerrotaReintentar;

    [Tooltip("Boton Menu de la pantalla de derrota.")]
    public Button botonDerrotaMenu;

    [Tooltip("Detiene el tiempo del juego cuando termina la partida.")]
    [FormerlySerializedAs("pauseGameOnEnd")]
    public bool pausarJuegoAlTerminar = true;

    bool gameEnded;

    void Awake()
    {
        Time.timeScale = 1f;
        HideFinalScreens();
    }

    void Start()
    {
        ResolveReferences();
        ConfigureFinalButtons();

        if (vidaJugador != null)
        {
            vidaJugador.PlayerDied += HandlePlayerDied;
        }
    }

    void OnDestroy()
    {
        if (vidaJugador != null)
        {
            vidaJugador.PlayerDied -= HandlePlayerDied;
        }
    }

    void Update()
    {
        if (gameEnded || sistemaOleadas == null)
        {
            return;
        }

        if (sistemaOleadas.LastWaveFinishedSpawning < oleadaParaGanar)
        {
            return;
        }

        if (sistemaOleadas.IsSpawningWave)
        {
            return;
        }

        if (CountActiveEnemies() > 0)
        {
            return;
        }

        EndGame(true);
    }

    void HandlePlayerDied()
    {
        EndGame(false);
    }

    void EndGame(bool jugadorGano)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        HideGameplayObjects();

        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(jugadorGano);
        }

        if (pantallaDerrota != null)
        {
            pantallaDerrota.SetActive(!jugadorGano);
        }

        Debug.Log("Fin del juego: " + (jugadorGano ? textoVictoria : textoDerrota));

        if (pausarJuegoAlTerminar)
        {
            Time.timeScale = 0f;
        }
    }

    int CountActiveEnemies()
    {
        return FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude).Length;
    }

    void ResolveReferences()
    {
        if (sistemaOleadas == null)
        {
            sistemaOleadas = FindAnyObjectByType<WaveManager>();
        }

        if (vidaJugador == null)
        {
            vidaJugador = FindAnyObjectByType<PlayerHealth>();
        }
    }

    void ConfigureFinalButtons()
    {
        ConfigureButton(botonVictoriaReintentar, RetryScene);
        ConfigureButton(botonDerrotaReintentar, RetryScene);
        ConfigureButton(botonVictoriaMenu, GoToMenu);
        ConfigureButton(botonDerrotaMenu, GoToMenu);
    }

    void ConfigureButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void HideFinalScreens()
    {
        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(false);
        }

        if (pantallaDerrota != null)
        {
            pantallaDerrota.SetActive(false);
        }
    }

    void HideGameplayObjects()
    {
        SetComponentObjectsActive<PlayerMovement>(false);
        SetComponentObjectsActive<PlayerAttack>(false);
        SetComponentObjectsActive<PlayerHealth>(false);
        SetComponentObjectsActive<EnemyAI>(false);
        SetComponentObjectsActive<Fireball>(false);
        SetComponentObjectsActive<WandPickup>(false);
        SetComponentObjectsActive<ItemPickupEffect>(false);
        SetComponentObjectsActive<ItemVisualEffect>(false);
        SetComponentObjectsActive<HealthPotionSpawner>(false);
        SetComponentObjectsActive<PlayerHealthHud>(false);
    }

    void SetComponentObjectsActive<T>(bool active) where T : Component
    {
        foreach (T component in FindObjectsByType<T>(FindObjectsInactive.Exclude))
        {
            component.gameObject.SetActive(active);
        }
    }

    void RetryScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(nombreEscenaMenu))
        {
            SceneManager.LoadScene(nombreEscenaMenu);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
