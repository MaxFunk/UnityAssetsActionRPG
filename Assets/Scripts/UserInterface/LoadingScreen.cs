using UnityEngine;
using UnityEngine.UIElements;

public class LoadingScreen : MonoBehaviour
{
    private enum FadeState
    {
        Idle,
        FadeIn,
        FadeOut
    }

    public float minDuration = 3f;
    public float fadeDuration = 0.5f;

    private float duration = 0f;
    private float fadeTime = 0f;
    private FadeState fadeState = FadeState.Idle;
    private UIDocument document;
    private VisualElement visualElement;


    void Start()
    {
        document = GetComponent<UIDocument>();
        visualElement = document.rootVisualElement;

        StartFadeIn();
        InputHandler.instance.inputBlocked = true;
    }


    void Update()
    {
        switch (fadeState) 
        {
            case FadeState.Idle:
                {
                    duration += Time.deltaTime;
                    if (duration > minDuration)
                        StartFadeOut();
                    break;
                }
            case FadeState.FadeIn:
                {
                    fadeTime += Time.deltaTime;
                    visualElement.style.opacity = visualElement.style.opacity.value + Time.deltaTime / fadeDuration;
                    if (fadeTime > fadeDuration)
                    {
                        visualElement.style.opacity = 1f;
                        fadeState = FadeState.Idle;
                    }
                        
                    break;
                }
            case FadeState.FadeOut:
                {
                    fadeTime += Time.deltaTime;
                    visualElement.style.opacity = visualElement.style.opacity.value - Time.deltaTime / fadeDuration;
                    if (fadeTime > fadeDuration)
                    {
                        visualElement.style.opacity = 0f;
                        RemoveSelf();
                    }
                    break;
                }
            default:
                break;
        }
    }

    private void StartFadeIn()
    {
        visualElement.style.opacity = 0f;
        fadeTime = 0f;
        fadeState = FadeState.FadeIn;
    }

    private void StartFadeOut()
    {
        visualElement.style.opacity = 1f;
        fadeTime = 0f;
        fadeState = FadeState.FadeOut;
    }

    private void RemoveSelf()
    {
        UserInterfaceManager.instance.loadingScreen = null;
        InputHandler.instance.inputBlocked = false;
        Destroy(gameObject);
    }
}
