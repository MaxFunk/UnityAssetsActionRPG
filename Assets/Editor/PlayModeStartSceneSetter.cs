using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneSetter
{
    static PlayModeStartSceneSetter()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/SceneStartScreen.unity");
        EditorSceneManager.playModeStartScene = scene;
    }
}

