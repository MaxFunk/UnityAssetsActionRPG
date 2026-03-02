using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartSceneSetter
{
    static PlayModeStartSceneSetter()
    {
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/StartScene.unity");
        EditorSceneManager.playModeStartScene = scene;
    }
}

