using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
public class LoadingZone : MonoBehaviour
{
    private SceneLoader loader;

    void Awake()
    {
        loader = GetComponent<SceneLoader>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var colliderPlayer = other.GetComponent<HeroCharacterController>();
        if (colliderPlayer != null && colliderPlayer.IsPlayerControlled)
        {
            loader.LoadScene();
        }
    }
}
