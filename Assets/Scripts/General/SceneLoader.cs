using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex = -1;
    public int spawnerIndex = -1;

    public void LoadScene()
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            return;

        float delay = UserInterfaceManager.instance.CreateLoadingScreen();
        StartCoroutine(DelayedCall(delay));

        GameManager.Instance.GameDataGeneral.sceneIndex = sceneIndex;
        GameManager.Instance.GameDataGeneral.sceneSpawner = spawnerIndex;

        SoundtrackManager.Instance.StopSoundtrack();
        SoundtrackManager.Instance.LoadAreaSoundtrack(-1); // clears area soundtrack
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        GameManager.Instance.CurSpawnerIndex = spawnerIndex;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator DelayedCall(float delay)
    {
        yield return new WaitForSeconds(delay);
        // yield return new WaitForSecondsRealtime(1f); // other option?

        CombatManager.Instance.Reset();
        StartCoroutine(LoadAsyncScene());
    }
}
