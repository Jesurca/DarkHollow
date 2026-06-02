using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    [Tooltip("Ruta de la escena del juego.")]
    public string escenaJuego = "Assets/Juego/Escenas/NivelJuego.unity";

    [Tooltip("Ruta de la escena de creditos.")]
    public string escenaCreditos = "Assets/Juego/Escenas/Creditos.unity";

    [Tooltip("Ruta de la escena de menu.")]
    public string escenaMenu = "Assets/Juego/Escenas/Menu.unity";

    public void Jugar()
    {
        LoadScene(escenaJuego);
    }

    public void Creditos()
    {
        LoadScene(escenaCreditos);
    }

    public void Salir()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void VolverMenu()
    {
        LoadScene(escenaMenu);
    }

    void LoadScene(string scenePathOrName)
    {
        Time.timeScale = 1f;

        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePathOrName);

        if (buildIndex >= 0)
        {
            SceneManager.LoadScene(buildIndex);
            return;
        }

        SceneManager.LoadScene(scenePathOrName);
    }
}
