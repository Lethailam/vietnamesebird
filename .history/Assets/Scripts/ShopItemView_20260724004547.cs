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

    [Header("Wing Animation")]
    [SerializeField] private UIBirdWingAnimator _uiWingAnimator;

    private BirdSkinData _skinData;
    private ShopGridManager _shopGridManager;

    private void Awake()
    {
        if (_uiWingAnimator == null)
        {
            _uiWingAnimator =
                GetComponentInChildren<UIBirdWingAnimator>();
        }
    }

    public void Setup(
        ShopGridManager manager,
        BirdSkinData skinData,
        bool isUnlocked,
        bool isSelected
    )
    {
        _shopGridManager = manager;
        _skinData = skinData;

        if (_skinData == null)
        {
            return;
        }

        if (_uiWingAnimator != null)
        {
            _uiWingAnimator.SetWingSprites(
                _skinData.wingUpSprite,
                _skinData.wingMidSprite,
                _skinData.wingDownSprite,
                true
            );
        }
        else if (_previewImage != null)
        {
            _previewImage.sprite =
                _skinData.wingMidSprite;
        }

        if (_priceText != null)
        {
            _priceText.text =
                _skinData.price.ToString();
        }

        if (_stateText != null)
        {
            if (isSelected)
            {
                _stateText.text = "SELECTED";
            }
            else if (isUnlocked)
            {
                _stateText.text = "OWNED";
            }
            else
            {
                _stateText.text = "LOCKED";
            }
        }

        if (_coinIcon != null)
        {
            _coinIcon.gameObject.SetActive(
                !isUnlocked
            );
        }

        if (_priceText != null)
        {
            _priceText.gameObject.SetActive(
                !isUnlocked
            );
        }

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(
                OnClickItem
            );
        }

        if (_setButton != null)
        {
            _setButton.gameObject.SetActive(
                isUnlocked && !isSelected
            );

            _setButton.onClick.RemoveAllListeners();
            _setButton.onClick.AddListener(
                OnClickSetButton
            );
        }

        if (_setButtonText != null)
        {
            _setButtonText.text = "SET";
        }
    }

    private void OnClickItem()
    {
        if (_shopGridManager == null ||
            _skinData == null)
        {
            return;
        }

        _shopGridManager.OnItemClicked(
            _skinData
        );
    }

    private void OnClickSetButton()
    {
        if (_shopGridManager == null ||
            _skinData == null)
        {
            return;
        }

        _shopGridManager.SetBird(
            _skinData
        );
    }
}