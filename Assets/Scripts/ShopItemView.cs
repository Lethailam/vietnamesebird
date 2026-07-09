using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _previewImage;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Image _coinIcon;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Button _button;
    [SerializeField] private Button _setButton;
    [SerializeField] private TextMeshProUGUI _setButtonText;

    private int _itemIndex;
    private ShopGridManager _shopGridManager;

    public void Setup(
        ShopGridManager manager,
        int itemIndex,
        Sprite previewSprite,
        int price,
        bool isUnlocked,
        bool isSelected)
    {
        _shopGridManager = manager;
        _itemIndex = itemIndex;

        if (_previewImage != null)
            _previewImage.sprite = previewSprite;

        if (_priceText != null)
            _priceText.text = price.ToString();

        if (_stateText != null)
        {
            if (isSelected)
                _stateText.text = "SELECT";
            else
                _stateText.text = isUnlocked ? "OWNED" : "LOCKED";
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClickItem);
        }

        if (_setButton != null)
        {
            _setButton.gameObject.SetActive(isUnlocked && !isSelected);

            _setButton.onClick.RemoveAllListeners();
            _setButton.onClick.AddListener(OnClickSetButton);
        }

        if (_setButtonText != null)
        {
            _setButtonText.text = "SET";
        }
    }

    private void OnClickItem()
    {
        if (_shopGridManager != null)
        {
            _shopGridManager.OnItemClicked(_itemIndex);
        }
    }

    private void OnClickSetButton()
    {
        if (_shopGridManager != null)
        {
            _shopGridManager.SetBird(_itemIndex);
        }
    }
}