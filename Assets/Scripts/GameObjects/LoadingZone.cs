using UnityEngine;

[RequireComponent(typeof(SceneLoader), typeof(PlayerChecker))]
public class LoadingZone : MonoBehaviour
{
    private SceneLoader loader;
    private PlayerChecker checker;


    void Awake()
    {
        loader = GetComponent<SceneLoader>();
        checker = GetComponent<PlayerChecker>();
    }

    
    void Update()
    {
        if (checker.checkActive && checker.CheckForPlayerCharacer())
        {
            loader.LoadScene();
        }
    }
}
