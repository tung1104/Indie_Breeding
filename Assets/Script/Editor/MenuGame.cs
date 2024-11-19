using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MenuGame : MonoBehaviour
{
    [MenuItem("Game/Play", false, 0)]
    public static void PlayGame()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/00-Loading.unity");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Game/Play", true, 0)]
    public static bool PlayGameValidate()
    {
        return !EditorApplication.isPlaying;
    }

    [MenuItem("Game/Stop", false, 1)]
    public static void StopGame()
    {
        EditorApplication.isPlaying = false;
    }

    [MenuItem("Game/Stop", true, 1)]
    public static bool StopGameValidate()
    {
        return EditorApplication.isPlaying;
    }

    [MenuItem("Game/00-Loading Scene", false, 100)]
    public static void OpenLoadingScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/00-Loading.unity");
    }

    [MenuItem("Game/01-Home Scene", false, 101)]
    public static void OpenHomeScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/01-Home.unity");
    }

    [MenuItem("Game/02-Wild Scene", false, 102)]
    public static void OpenWildScene2()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/02-Wild.unity");
    }

    [MenuItem("Game/Test Scene", false, 103)]
    public static void OpenTestScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
    }
}
