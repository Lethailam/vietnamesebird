using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMapManager : MonoBehaviour
{
    public static BackgroundMapManager Instance;

    [System.Serializable]
    public class LocationData
    {
        public string locationName;
        public Sprite previewSprite;
        public Sprite backgroundSprite;
        public Button mapButton;
        public GameObject lockObject;
    }

    [Header("Locations")]
    [SerializeField] private LocationData[] _locations;

    [Header("Popup UI")]
    [SerializeField] private GameObject _backgroundPopup;
    [SerializeField] private Image _previewImage;
    [SerializeField] private GameObject _dimOverlay;
    [SerializeField] private TextMeshProUGUI _locationNameText;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private TextMeshProUGUI _unlockRequirementText;
    [SerializeField] private Button _setBackgroundButton;
    [SerializeField] private TextMeshProUGUI _setButtonText;
    [SerializeField] private Button _closePopupButton;

    [Header("Map UI")]
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Game Background Target")]
    [SerializeField] private SpriteRenderer _mainBackgroundRenderer;

    [Header("Unlock Settings")]
    [SerializeField] private int _scorePerUnlock = 100;

    private const string SELECTED_BG_KEY = "SelectedBackgroundIndex";
    private int _currentPreviewIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildButtons();
        RefreshUnlockUI();
        ApplySelectedBackgroundToGame();

        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(false);
    }

    private void BuildButtons()
    {
        for (int i = 0; i < _locations.Length; i++)
        {
            int index = i;

            if (_locations[i].mapButton != null)
            {
                _locations[i].mapButton.onClick.RemoveAllListeners();
                _locations[i].mapButton.onClick.AddListener(() => OpenLocationPopup(index));
            }
        }

        if (_setBackgroundButton != null)
        {
            _setBackgroundButton.onClick.RemoveAllListeners();
            _setBackgroundButton.onClick.AddListener(SetCurrentPreviewAsMainBackground);
        }

        if (_closePopupButton != null)
        {
            _closePopupButton.onClick.RemoveAllListeners();
            _closePopupButton.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenLocationPopup(int index)
    {
        if (index < 0 || index >= _locations.Length) return;

        _currentPreviewIndex = index;

        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(true);

        UpdatePopupUI();
    }

    public void ClosePopup()
    {
        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(false);
    }

    private void UpdatePopupUI()
    {
        if (_currentPreviewIndex < 0 || _currentPreviewIndex >= _locations.Length) return;

        LocationData data = _locations[_currentPreviewIndex];
        bool isUnlocked = IsLocationUnlocked(_currentPreviewIndex);
        bool isSelected = PlayerPrefs.GetInt(SELECTED_BG_KEY, 0) == _currentPreviewIndex;

        if (_previewImage != null)
            _previewImage.sprite = data.previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = data.locationName;

        if (_dimOverlay != null)
            _dimOverlay.SetActive(!isUnlocked);

        if (_stateText != null)
        {
            if (!isUnlocked)
                _stateText.text = "LOCKED";
            else if (isSelected)
                _stateText.text = "SELECTED";
            else
                _stateText.text = "UNLOCKED";
        }

        if (_unlockRequirementText != null)
        {
            if (isUnlocked)
            {
                _unlockRequirementText.text = "UNLOCKED";
            }
            else
            {
                int requiredScore = _currentPreviewIndex * _scorePerUnlock;
                _unlockRequirementText.text = "Unlock at " + requiredScore + " points";
            }
        }

        if (_setBackgroundButton != null)
            _setBackgroundButton.interactable = isUnlocked && !isSelected;

        if (_setButtonText != null)
        {
            if (!isUnlocked)
                _setButtonText.text = "LOCKED";
            else if (isSelected)
                _setButtonText.text = "SELECTED";
            else
                _setButtonText.text = "SET BACKGROUND";
        }
    }

    public void SetCurrentPreviewAsMainBackground()
    {
        if (!IsLocationUnlocked(_currentPreviewIndex))
            return;

        PlayerPrefs.SetInt(SELECTED_BG_KEY, _currentPreviewIndex);
        PlayerPrefs.Save();

        ApplySelectedBackgroundToGame();
        RefreshUnlockUI();
        UpdatePopupUI();
    }

    public void ApplySelectedBackgroundToGame()
    {
        int selectedIndex = PlayerPrefs.GetInt(SELECTED_BG_KEY, 0);

        if (_mainBackgroundRenderer != null &&
            selectedIndex >= 0 &&
            selectedIndex < _locations.Length)
        {
            _mainBackgroundRenderer.sprite = _locations[selectedIndex].backgroundSprite;
        }
    }

    public void RefreshUnlockUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int unlockedCount = GetUnlockedLocationCount(highScore);

        for (int i = 0; i < _locations.Length; i++)
        {
            bool isUnlocked = i < unlockedCount;

            if (_locations[i].lockObject != null)
                _locations[i].lockObject.SetActive(!isUnlocked);
        }

        if (_progressText != null)
        {
            int nextUnlockScore = GetNextUnlockScore(highScore);

            if (nextUnlockScore < 0)
            {
                _progressText.text = "ALL LOCATIONS UNLOCKED";
            }
            else
            {
                _progressText.text = "Điểm cao nhất: " + highScore +
                                     "\nMở khóa tiếp theo tại: " + nextUnlockScore;
            }
        }
    }

    private bool IsLocationUnlocked(int index)
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int unlockedCount = GetUnlockedLocationCount(highScore);
        return index >= 0 && index < unlockedCount;
    }

    private int GetUnlockedLocationCount(int highScore)
    {
        int unlocked = 1 + (highScore / _scorePerUnlock);
        return Mathf.Clamp(unlocked, 1, _locations.Length);
    }

    private int GetNextUnlockScore(int highScore)
    {
        int unlockedCount = GetUnlockedLocationCount(highScore);

        if (unlockedCount >= _locations.Length)
            return -1;

        return unlockedCount * _scorePerUnlock;
    }
}