using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex = -1;

    public void LoadScene()
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            return;

        float delay = UserInterfaceManager.instance.CreateLoadingScreen();
        StartCoroutine(DelayedCall(delay));
        SoundtrackManager.Instance.StopSoundtrack();
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        // Wait until the asynchronous scene fully loads
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
