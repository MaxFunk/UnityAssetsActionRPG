using UnityEngine;
using UnityEngine.Splines;

public class MinigamePath : MonoBehaviour
{
    public SplineContainer splinePath;
    public Transform railCar;
    public Camera railCamera;

    private bool minigameActive = false;
    private float pathProgress = 0f;
    private float velocity = 0f;

    void Update()
    {
        if (!minigameActive) return;

        var input = InputHandler.instance.GetMoveInput().z;
        velocity += input * Time.deltaTime * 0.2f;
        velocity -= 0.05f * Time.deltaTime;
        velocity = Mathf.Clamp(velocity, 0f, 0.2f);

        pathProgress += velocity * Time.deltaTime;
        pathProgress %= 1f;

        railCar.transform.position = splinePath.EvaluatePosition(pathProgress);
        Vector3 forward = splinePath.EvaluateTangent(pathProgress);
        railCar.transform.rotation = Quaternion.LookRotation(forward);

        if (InputHandler.instance.GetSheatheWeaponInputDown())
            EndMinigame();
    }

    public void StartMinigame()
    {
        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.characterState = HeroCharacterController.CharacterState.Stopped;
        }

        railCamera.enabled = true;
        minigameActive = true;
    }


    public void EndMinigame()
    {
        railCamera.enabled = false;
        minigameActive = false;

        var heroChars = FindObjectsByType<HeroCharacterController>(FindObjectsSortMode.None);
        foreach (var character in heroChars)
        {
            if (character != null)
                character.characterState = HeroCharacterController.CharacterState.Explore;
        }
    }
}
