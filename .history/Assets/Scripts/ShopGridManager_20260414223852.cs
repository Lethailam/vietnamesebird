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
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    [Header("Bird Shop Data")]
    [SerializeField] private Sprite[] _birdSprites;
    [SerializeField] private int[] _birdPrices;

    private const string SELECTED_BIRD_KEY = "SelectedBird";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        // Chim đầu tiên luôn mở sẵn
        PlayerPrefs.SetInt(GetBirdUnlockKey(0), 1);

        LoadSelectedBird();
        RefreshCoinText();
    }

    public void OpenBirdShop()
    {
        if (_shopPanel != null)
            _shopPanel.SetActive(true);

        RefreshCoinText();
        BuildShop();
    }

    public void CloseShop()
    {
        if (_shopPanel != null)
            _shopPanel.SetActive(false);
    }

    public void BuildShop()
    {
        if (_content == null || _itemPrefab == null)
        {
            Debug.LogError("ShopGridManager: Missing Content or Item Prefab.");
            return;
        }

        if (_birdSprites == null || _birdPrices == null)
        {
            Debug.LogError("ShopGridManager: Bird data is missing.");
            return;
        }

        if (_birdSprites.Length != _birdPrices.Length)
        {
            Debug.LogError("ShopGridManager: _birdSprites.Length must equal _birdPrices.Length");
            return;
        }

        // Xóa item cũ
        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(_content.GetChild(i).gameObject);
        }

        // Tạo item mới
        for (int i = 0; i < _birdSprites.Length; i++)
        {
            bool isUnlocked = IsBirdUnlocked(i);

            ShopItemView item = Instantiate(_itemPrefab, _content);
            item.Setup(this, i, _birdSprites[i], _birdPrices[i], isUnlocked);
        }
    }

    public void OnItemClicked(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _birdSprites.Length) return;

        bool isUnlocked = IsBirdUnlocked(itemIndex);

        // Nếu chưa mua thì thử mua bằng coin
        if (!isUnlocked)
        {
            int price = _birdPrices[itemIndex];

            if (CoinWallet.Instance == null)
            {
                Debug.LogError("CoinWallet instance is missing.");
                return;
            }

            bool bought = CoinWallet.Instance.TrySpendCoins(price);
            if (!bought)
            {
                Debug.Log("Not enough coins.");
                return;
            }

            PlayerPrefs.SetInt(GetBirdUnlockKey(itemIndex), 1);
            PlayerPrefs.Save();
        }

        // Sau khi mua xong hoặc đã mua sẵn -> equip luôn chim đó
        EquipBird(itemIndex);

        RefreshCoinText();
        BuildShop();
    }

    private void EquipBird(int itemIndex)
    {
        PlayerPrefs.SetInt(SELECTED_BIRD_KEY, itemIndex);
        PlayerPrefs.Save();

        if (_playerSpriteRenderer != null &&
            itemIndex >= 0 &&
            itemIndex < _birdSprites.Length)
        {
            _playerSpriteRenderer.sprite = _birdSprites[itemIndex];
        }
    }

    private void LoadSelectedBird()
    {
        int selectedBird = PlayerPrefs.GetInt(SELECTED_BIRD_KEY, 0);

        if (_playerSpriteRenderer != null &&
            selectedBird >= 0 &&
            selectedBird < _birdSprites.Length)
        {
            _playerSpriteRenderer.sprite = _birdSprites[selectedBird];
        }
    }

    private bool IsBirdUnlocked(int birdIndex)
    {
        if (birdIndex == 0) return true;
        return PlayerPrefs.GetInt(GetBirdUnlockKey(birdIndex), 0) == 1;
    }

    private string GetBirdUnlockKey(int birdIndex)
    {
        return "BirdUnlocked_" + birdIndex;
    }

    public void RefreshCoinText()
    {
        if (_coinAmountText != null && CoinWallet.Instance != null)
        {
            _coinAmountText.text = CoinWallet.Instance.GetCoins().ToString();
        }
    }
}