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

    [Header("Bird Shop Data")]
    [SerializeField] private Sprite[] _birdSprites;
    [SerializeField] private int[] _birdPrices;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    if (_shopPanel != null)
        _shopPanel.SetActive(false);

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
            ShopItemView item = Instantiate(_itemPrefab, _content);
            item.Setup(this, i, _birdSprites[i], _birdPrices[i]);
        }
    }

    public void OnItemClicked(int itemIndex)
    {
        Debug.Log("Clicked item: " + itemIndex);

        // Tạm thời chỉ log.
        // Sau này sẽ thêm logic mua / equip chim ở đây.
    }

    public void RefreshCoinText()
    {
        if (_coinAmountText != null && CoinWallet.Instance != null)
        {
            _coinAmountText.text = CoinWallet.Instance.GetCoins().ToString();
        }
    }
}