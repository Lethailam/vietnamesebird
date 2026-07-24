using UnityEngine;

[System.Serializable]
public class BirdSkinData
{
    [Header("Skin Info")]
    public string skinId;
    public string skinName;
    public int price;

    [Header("Wing Sprites")]
    public Sprite wingUpSprite;
    public Sprite wingMidSprite;
    public Sprite wingDownSprite;

    [Header("Shop")]
    public bool unlockedByDefault;
}