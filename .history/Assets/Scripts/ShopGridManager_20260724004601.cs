using TMPro;
using UnityEngine;

public class ShopGridManager : MonoBehaviour
{
    public static ShopGridManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private Transform _content;
    [SerializeField] private ShopItemView _itemPrefab;
    [SerializeField] private TextMeshProUGUI _coinAmountText;

    [Header("Current Bird Preview")]
    [Tooltip("SpriteRenderer của chim preview ở màn hình chính nếu có.")]
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    [Tooltip("BirdWingAnimator của chim preview ở màn hình chính nếu có.")]
    [SerializeField] private BirdWingAnimator _playerWingAnimator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }

        RefreshCurrentBirdPreview();
        RefreshCoinText();
    }

    public void OpenBirdShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
        }

        RefreshCoinText();
        BuildShop();
    }

    public void CloseShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
        }
    }

    public void BuildShop()
    {
        if (_content == null ||
            _itemPrefab == null)
        {
            Debug.LogError(
                "ShopGridManager: Missing Content or Item Prefab."
            );

            return;
        }

        if (BirdSkinManager.Instance == null)
        {
            Debug.LogError(
                "ShopGridManager: Không tìm thấy BirdSkinManager.Instance."
            );

            return;
        }

        BirdSkinData[] birdSkins =
            BirdSkinManager.Instance.GetAllSkins();

        if (birdSkins == null ||
            birdSkins.Length == 0)
        {
            Debug.LogError(
                "ShopGridManager: BirdSkinManager chưa có dữ liệu chim."
            );

            return;
        }

        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(
                _content.GetChild(i).gameObject
            );
        }

        BirdSkinData selectedSkin =
            BirdSkinManager.Instance.GetSelectedSkin();

        for (int i = 0; i < birdSkins.Length; i++)
        {
            BirdSkinData skin =
                birdSkins[i];

            if (skin == null)
            {
                continue;
            }

            bool isUnlocked =
                BirdSkinManager.Instance.IsUnlocked(
                    skin.skinId
                );

            bool isSelected =
                selectedSkin != null &&
                selectedSkin.skinId == skin.skinId;

            ShopItemView item =
                Instantiate(
                    _itemPrefab,
                    _content
                );

            item.Setup(
                this,
                skin,
                isUnlocked,
                isSelected
            );
        }
    }

    public void OnItemClicked(
        BirdSkinData skinData
    )
    {
        if (skinData == null)
        {
            return;
        }

        if (BirdSkinManager.Instance == null)
        {
            Debug.LogError(
                "ShopGridManager: Không tìm thấy BirdSkinManager.Instance."
            );

            return;
        }

        bool isUnlocked =
            BirdSkinManager.Instance.IsUnlocked(
                skinData.skinId
            );

        if (!isUnlocked)
        {
            if (CoinWallet.Instance == null)
            {
                Debug.LogError(
                    "CoinWallet instance is missing."
                );

                return;
            }

            bool bought =
                CoinWallet.Instance.TrySpendCoins(
                    skinData.price
                );

            if (!bought)
            {
                Debug.Log(
                    "Không đủ coin để mua chim."
                );

                return;
            }

            BirdSkinManager.Instance.UnlockSkin(
                skinData.skinId
            );

            Debug.Log(
                "Đã mua chim: " + skinData.skinId
            );
        }

        RefreshCoinText();
        BuildShop();
    }

    public void SetBird(
        BirdSkinData skinData
    )
    {
        if (skinData == null ||
            BirdSkinManager.Instance == null)
        {
            return;
        }

        bool isUnlocked =
            BirdSkinManager.Instance.IsUnlocked(
                skinData.skinId
            );

        if (!isUnlocked)
        {
            Debug.LogWarning(
                "Chim chưa được mở khóa: " +
                skinData.skinId
            );

            return;
        }

        BirdSkinManager.Instance.SelectSkin(
            skinData.skinId
        );

        RefreshCurrentBirdPreview();
        BuildShop();
    }

    private void RefreshCurrentBirdPreview()
    {
        if (BirdSkinManager.Instance == null)
        {
            return;
        }

        BirdSkinData selectedSkin =
            BirdSkinManager.Instance.GetSelectedSkin();

        if (selectedSkin == null)
        {
            return;
        }

        if (_playerWingAnimator != null)
        {
            _playerWingAnimator.SetWingSprites(
                selectedSkin.wingUpSprite,
                selectedSkin.wingMidSprite,
                selectedSkin.wingDownSprite,
                true
            );
        }
        else if (_playerSpriteRenderer != null)
        {
            _playerSpriteRenderer.sprite =
                selectedSkin.wingMidSprite;
        }
    }

    public void RefreshCoinText()
    {
        if (_coinAmountText != null &&
            CoinWallet.Instance != null)
        {
            _coinAmountText.text =
                CoinWallet.Instance.GetCoins().ToString();
        }
    }
}