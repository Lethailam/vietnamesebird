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
        if (_content == null || _itemPrefab == null) return;
        if (_birdSprites == null || _birdPrices == null) return;
        if (_birdSprites.Length != _birdPrices.Length) return;

        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(_content.GetChild(i).gameObject);
        }

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

        bool unlocked = IsBirdUnlocked(itemIndex);

        if (!unlocked)
        {
            int price = _birdPrices[itemIndex];

            if (CoinWallet.Instance != null && CoinWallet.Instance.TrySpendCoins(price))
            {
                PlayerPrefs.SetInt(GetBirdUnlockKey(itemIndex), 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("Not enough coins to buy bird " + itemIndex);
                return;
            }
        }

        EquipBird(itemIndex);
        RefreshCoinText();
        BuildShop();
    }

    private void EquipBird(int itemIndex)
    {
        PlayerPrefs.SetInt(SELECTED_BIRD_KEY, itemIndex);
        PlayerPrefs.Save();

        if (_playerSpriteRenderer != null && itemIndex >= 0 && itemIndex < _birdSprites.Length)
        {
            _playerSpriteRenderer.sprite = _birdSprites[itemIndex];
        }
    }

    private void LoadSelectedBird()
    {
        PlayerPrefs.SetInt(GetBirdUnlockKey(0), 1);

        int selectedBird = PlayerPrefs.GetInt(SELECTED_BIRD_KEY, 0);

        if (_playerSpriteRenderer != null &&
            selectedBird >= 0 &&
            selectedBird < _birdSprites.Length)
        {
            _playerSpriteRenderer.sprite = _birdSprites[selectedBird];
        }
    }

    private bool IsBirdUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt(GetBirdUnlockKey(index), 0) == 1;
    }

    private string GetBirdUnlockKey(int index)
    {
        return "BirdUnlocked_" + index;
    }

    public void RefreshCoinText()
    {
        if (_coinAmountText != null && CoinWallet.Instance != null)
        {
            _coinAmountText.text = CoinWallet.Instance.GetCoins().ToString();
        }
    }
}