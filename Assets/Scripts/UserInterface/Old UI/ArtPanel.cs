using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ArtPanel : MonoBehaviour
{
    public RectTransform CooldownIcon;
    public float maxHeightCooldownIcon;
    public TextMeshProUGUI ArtNameText;

    private ArtData linkedArtData = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (linkedArtData != null)
        {
            float iconHeight = maxHeightCooldownIcon * 1.0f;// linkedArtData.GetCooldownPercentage();
            CooldownIcon.sizeDelta = new Vector2(CooldownIcon.sizeDelta.x, iconHeight);
        }
    }

    public void LinkData(ArtData data)
    {
        linkedArtData = data;
        ArtNameText.text = data.artName;
    }
}
