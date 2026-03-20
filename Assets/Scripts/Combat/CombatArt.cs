using UnityEngine;

public class CombatArt
{
    public ArtData artData = null;
    public bool onCooldown = false;
    public float timerCooldown = 0.0f;
    public int ultPoints = 0;

    public int artLevel = 0; // 0 - 4
    public float curArtCooldown = 0f;
    public float curBasePower = 0;
    public float inRangeOpacity = 1f;

    private CombatData owner;
    private bool inRange = false;
    private bool valid = false;

    public void ExternalUpdate()
    {
        if (!valid) return;

        // check in range -> get target, check with owner range, set in range (for ui and check)
        inRange = artData.IsInCastRange(owner, owner.GetCurrentTarget());
        inRangeOpacity = inRange ? 1f : 0.66f;

        if (onCooldown)
        {
            timerCooldown += Time.deltaTime;
            if (timerCooldown >= curArtCooldown)
            {
                onCooldown = false;
                timerCooldown = curArtCooldown;
            }
        }
    }


    public void LoadData(CombatData owner, int artIndex, int artLevel)
    {
        this.owner = owner;
        var newData = ScriptableManager.instance.GetArtData(artIndex);
        if ( (artData != null && artData == newData) || newData == null) return;

        this.artLevel = Mathf.Clamp(artLevel, 0, 4);
        artData = newData;
        curBasePower = artData.basePower[artLevel];
        curArtCooldown = artData.artCooldown[artLevel];

        onCooldown = false;
        valid = true;
        timerCooldown = curArtCooldown;
    }

    public void ResetCooldown()
    {
        if (artData != null)
        {
            onCooldown = true;
            timerCooldown = 0.0f;
        }
    }


    public bool CanCastArt()
    {
        if (artData == null || artData.ArtCastPrefab == null || onCooldown || !inRange || !valid)
            return false;

        if (artData.isUlt)
            return ultPoints >= artData.ultCost;

        return true;
    }

    public void OnCastArt()
    {
        if (onCooldown || artData == null || artData.ArtCastPrefab == null)
            return;

        if (artData.isUlt)
        {
            ultPoints = 0;
            curArtCooldown = artData.ultCost;
        }
        else
            onCooldown = true;

        timerCooldown = 0.0f;
    }

    public bool IsOnCooldown()
    { 
        return onCooldown; 
    }

    public void IncreaseUltPoints(int amount)
    {
        if (artData == null || artData.isUlt == false) return;
        ultPoints = Mathf.Clamp(ultPoints + amount, 0, artData.ultCost);
        timerCooldown = ultPoints;
        curArtCooldown = artData.ultCost;
    }

    public float GetBasePower()
    {
        if (artData == null)
            return 0f;
        return artData.basePower[artLevel];
    }
}
