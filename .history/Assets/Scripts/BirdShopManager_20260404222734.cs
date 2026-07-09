using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BirdShopManager : MonoBehaviour
{
    public static BirdShopManager Instance;

    [Header("Bird Data")]
    [SerializeField] private Sprite[] _birdSprites;
    [SerializeField] private int[] _birdPrices;

    [Header("UI")]
    [SerializeField] private GameObject _levelCompletePanel;
    [SerializeField] private Image _birdPreviewImage;
    [SerializeField] private TextMeshProUGUI _birdPriceText;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _selectButton;
    [SerializeField] private TextMeshProUGUI _buyButtonText;
    [SerializeField] private TextMeshProUGUI _selectButtonText;

    [Header("Player")]
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    private int _currentBirdIndexToOffer = 1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadSelectedBird();
        _levelCompletePanel.SetActive(false);
    }

    public void ShowBirdOfferForLevel(int levelIndex)
    {
        _currentBirdIndexToOffer = levelIndex;

        if (_currentBirdIndexToOffer >= _birdSprites.Length)
        {
            _levelCompletePanel.SetActive(true);
            _birdPreviewImage.gameObject.SetActive(false);
            _birdPriceText.text = "All birds unlocked";
            _buyButton.gameObject.SetActive(false);
            _selectButton.gameObject.SetActive(false);
            return;
        }

        _levelCompletePanel.SetActive(true);
        _birdPreviewImage.gameObject.SetActive(true);

        _birdPreviewImage.sprite = _birdSprites[_currentBirdIndexToOffer];
        _birdPriceText.text = "Price: " + _birdPrices[_currentBirdIndexToOffer];

        bool unlocked = IsBirdUnlocked(_currentBirdIndexToOffer);

        _buyButton.gameObject.SetActive(!unlocked);
        _selectButton.gameObject.SetActive(unlocked);

        _buyButtonText.text = "Buy";
        _selectButtonText.text = IsSelectedBird(_currentBirdIndexToOffer) ? "Selected" : "Select";
    }

    public void BuyBird()
    {
        int currentScore = Score.instance.GetScore();
        int price = _birdPrices[_currentBirdIndexToOffer];

        if (currentScore >= price)
        {
            Score.instance.SpendScore(price);
            PlayerPrefs.SetInt("BirdUnlocked_" + _currentBirdIndexToOffer, 1);

            _buyButton.gameObject.SetActive(false);
            _selectButton.gameObject.SetActive(true);
            _selectButtonText.text = "Select";
        }
    }

    public void SelectBird()
    {
        if (!IsBirdUnlocked(_currentBirdIndexToOffer)) return;

        PlayerPrefs.SetInt("SelectedBird", _currentBirdIndexToOffer);
        _playerSpriteRenderer.sprite = _birdSprites[_currentBirdIndexToOffer];
        _selectButtonText.text = "Selected";
    }

    public void HidePanel()
    {
        _levelCompletePanel.SetActive(false);
    }

    private void LoadSelectedBird()
    {
        int selectedBird = PlayerPrefs.GetInt("SelectedBird", 0);

        if (selectedBird >= 0 && selectedBird < _birdSprites.Length)
        {
            _playerSpriteRenderer.sprite = _birdSprites[selectedBird];
        }
    }

    private bool IsBirdUnlocked(int birdIndex)
    {
        if (birdIndex == 0) return true;
        return PlayerPrefs.GetInt("BirdUnlocked_" + birdIndex, 0) == 1;
    }

    private bool IsSelectedBird(int birdIndex)
    {
        return PlayerPrefs.GetInt("SelectedBird", 0) == birdIndex;
    }
}