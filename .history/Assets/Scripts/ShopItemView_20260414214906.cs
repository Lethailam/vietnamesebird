using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField] private Image _previewImage;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _coinIcon;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Button _button;

    private int _itemIndex;
    private ShopGridManager _shopGridManager;

    public void Setup(
        ShopGridManager manager,
        int itemIndex,
        Sprite previewSprite,
        int price,
        bool isUnlocked)
    {
        _shopGridManager = manager;
        _itemIndex = itemIndex;

        if (_previewImage != null)
            _previewImage.sprite = previewSprite;

        if (_priceText != null)
            _priceText.text = price.ToString();

        if (_stateText != null)
            _stateText.text = isUnlocked ? "OWNED" : "LOCKED";

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClickItem);
        }
    }

    private void OnClickItem()
    {
        if (_shopGridManager != null)
        {
            _shopGridManager.OnItemClicked(_itemIndex);
        }
    }
}