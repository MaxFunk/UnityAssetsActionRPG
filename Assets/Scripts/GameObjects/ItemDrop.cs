using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(PlayerChecker), typeof(CharacterController))]
public class ItemDrop : MonoBehaviour
{
    public ItemRecieveData dropData = null;
    public bool autoCollect = false;
    public bool doDespawn = true;
    public float despawnAfter = 10f;
    public Vector3 initialVelocity = Vector3.zero;

    private PlayerChecker checker;
    private CharacterController controller;

    private Vector3 objectVelocity;
    private float gravityValue = -2f;
    private float timeAlive = 0f;
    private bool isDespawning = false;
    private float scaleFactor = 2f;


    void Awake()
    {
        checker = GetComponent<PlayerChecker>();
        controller = GetComponent<CharacterController>();
        objectVelocity = initialVelocity;
    }

    
    void Update()
    {
        if (isDespawning)
        {
            gameObject.transform.localScale -= scaleFactor * Time.deltaTime * Vector3.one;
            if (gameObject.transform.localScale.x < 0.05f)
            {
                Destroy(this.gameObject);
            }
            return;
        }

        if (controller.isGrounded)
        {
            var valid = checker.CheckForPlayerCharacer();
            if (valid) 
            {
                GameManager.Instance.ItemManager.RecieveItems(dropData);
                Despawn();
                return;
            }

            timeAlive += Time.deltaTime;
            if (timeAlive > despawnAfter)
            {
                if (autoCollect)
                    GameManager.Instance.ItemManager.RecieveItems(dropData);
                Despawn();
            }
        }
        else
        {
            objectVelocity.y += gravityValue * Time.deltaTime;
            var finalVelocity = objectVelocity; // (objectVelocity.y * Vector3.up);
            controller.Move(finalVelocity * Time.deltaTime);
        }
    }

    private void Despawn()
    {
        isDespawning = true;
        //Destroy(this.gameObject, scaleFactor);
    }

    public void SetInitialVelocity(Vector3 initVelocity)
    {
        initialVelocity = initVelocity;
        objectVelocity = initialVelocity;
    }
}
