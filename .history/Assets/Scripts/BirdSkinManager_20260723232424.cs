using UnityEngine;

public class BirdSkinManager : MonoBehaviour
{
    public static BirdSkinManager Instance { get; private set; }

    [Header("All Bird Skins")]
    [SerializeField] private BirdSkinData[] _birdSkins;

    private const string SelectedSkinKey =
        "SelectedBirdSkinId";

    private const string UnlockedSkinPrefix =
        "BirdSkinUnlocked_";

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UnlockDefaultSkins();
    }

    private void UnlockDefaultSkins()
    {
        if (_birdSkins == null)
        {
            return;
        }

        for (int i = 0; i < _birdSkins.Length; i++)
        {
            BirdSkinData skin = _birdSkins[i];

            if (skin == null)
            {
                continue;
            }

            if (skin.unlockedByDefault)
            {
                UnlockSkin(skin.skinId);
            }
        }

        if (!PlayerPrefs.HasKey(SelectedSkinKey) &&
            _birdSkins.Length > 0 &&
            _birdSkins[0] != null)
        {
            SelectSkin(_birdSkins[0].skinId);
        }
    }

    public BirdSkinData[] GetAllSkins()
    {
        return _birdSkins;
    }

    public BirdSkinData GetSelectedSkin()
    {
        string selectedSkinId =
            PlayerPrefs.GetString(
                SelectedSkinKey,
                ""
            );

        BirdSkinData selectedSkin =
            GetSkinById(selectedSkinId);

        if (selectedSkin != null)
        {
            return selectedSkin;
        }

        if (_birdSkins != null &&
            _birdSkins.Length > 0)
        {
            return _birdSkins[0];
        }

        return null;
    }

    public BirdSkinData GetSkinById(
        string skinId
    )
    {
        if (_birdSkins == null)
        {
            return null;
        }

        for (int i = 0; i < _birdSkins.Length; i++)
        {
            BirdSkinData skin = _birdSkins[i];

            if (skin == null)
            {
                continue;
            }

            if (skin.skinId == skinId)
            {
                return skin;
            }
        }

        return null;
    }

    public bool IsUnlocked(
        string skinId
    )
    {
        return PlayerPrefs.GetInt(
            UnlockedSkinPrefix + skinId,
            0
        ) == 1;
    }

    public void UnlockSkin(
        string skinId
    )
    {
        PlayerPrefs.SetInt(
            UnlockedSkinPrefix + skinId,
            1
        );

        PlayerPrefs.Save();
    }

    public void SelectSkin(
        string skinId
    )
    {
        if (!IsUnlocked(skinId))
        {
            Debug.LogWarning(
                "Skin chưa được mở khóa: " + skinId
            );

            return;
        }

        PlayerPrefs.SetString(
            SelectedSkinKey,
            skinId
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Đã chọn skin chim: " + skinId
        );
    }
}