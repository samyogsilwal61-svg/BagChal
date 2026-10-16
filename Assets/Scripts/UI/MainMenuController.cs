using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Button actions for the Main Menu scene.
/// Put this on an empty GameObject called "MainMenuController" in the MainMenu scene,
/// then hook its public methods to the buttons' On Click lists.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene names (must match the scene files and Build Settings exactly)")]
    [SerializeField] private string gameSceneName = "GameScene";

    /// <summary>Player vs Player on one device. Hook to Button_Offline.</summary>
    public void PlayLocal()
    {
        LoadScene(gameSceneName);
    }

    /// <summary>Placeholder for room-code online play. Hook to Button_Online later.</summary>
    public void PlayOnline()
    {
        Debug.Log("Online play is not built yet.");
    }

    /// <summary>Quit the app. Hook to Button_Exit. Does nothing in the editor or in WebGL.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
        Application.Quit();
#endif
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("MainMenuController: scene '" + sceneName +
                           "' is missing from Build Settings or the name is wrong.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}