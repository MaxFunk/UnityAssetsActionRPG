using UnityEngine;

public class ItemOrb : MonoBehaviour
{
    public ItemRecieveData[] itemDrops;
    public float timeToDestroy = 0.5f;

    private bool isDestroying = false;
    private float timeLeft = 0.5f;
    private Light lightOrb;

    private Vector3 startScale = Vector3.zero;
    private float lightIntensity = 1f;

    private void Awake()
    {
        lightOrb = GetComponent<Light>();
        startScale = transform.localScale;
        lightIntensity = lightOrb.intensity;
    }

    private void Update()
    {
        if (isDestroying)
        {
            timeLeft -= Time.deltaTime;
            var lerpValue = timeLeft / timeToDestroy;
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, lerpValue);
            lightOrb.intensity = Mathf.Lerp(0, lightIntensity, lerpValue);

            if (timeLeft < 0)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroying) return;

        foreach (var itemDrop in itemDrops)
        {
            if (Random.value < itemDrop.recieveChance)
                GameManager.Instance.ItemManager.RecieveItems(itemDrop);
        }
        isDestroying = true;
        timeLeft = timeToDestroy;
    }
}
