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

    [Header("Map UI")]
    [SerializeField] private GameObject _leftMapArea;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Popup UI")]
    [SerializeField] private GameObject _backgroundPopup;
    [SerializeField] private Image _previewImage;
    [SerializeField] private GameObject _dimOverlay;
    [SerializeField] private TextMeshProUGUI _locationNameText;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private TextMeshProUGUI _unlockRequirementText;
    [SerializeField] private Button _closePopupButton;

    [Header("Game Background Target")]
    [SerializeField] private SpriteRenderer _mainBackgroundRenderer;

    [Header("Unlock Settings")]
    [SerializeField] private int _scorePerUnlock = 100;

    private int _currentPreviewIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildButtons();
        RefreshUnlockUI();
        ApplyProgressBackground();

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

        if (_leftMapArea != null)
            _leftMapArea.SetActive(false);

        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(true);

        UpdatePopupUI();
    }

    public void ClosePopup()
    {
        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(false);

        if (_leftMapArea != null)
            _leftMapArea.SetActive(true);
    }

    public void ResetMapUI()
    {
        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(false);

        if (_leftMapArea != null)
            _leftMapArea.SetActive(true);

        RefreshUnlockUI();
    }

    private void UpdatePopupUI()
    {
        if (_currentPreviewIndex < 0 || _currentPreviewIndex >= _locations.Length) return;

        LocationData data = _locations[_currentPreviewIndex];
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        int currentIndex = GetCurrentLocationIndex(highScore);
        int nextIndex = GetNextLocationIndex(highScore);

        bool isCurrent = _currentPreviewIndex == currentIndex;
        bool isNext = _currentPreviewIndex == nextIndex;
        bool isPassed = _currentPreviewIndex < currentIndex;
        bool isLocked = _currentPreviewIndex > currentIndex;

        if (_previewImage != null)
            _previewImage.sprite = data.previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = data.locationName.ToUpper();

        if (_dimOverlay != null)
            _dimOverlay.SetActive(isLocked && !isNext);

        if (_stateText != null)
        {
            if (isCurrent)
                _stateText.text = "CURRENT";
            else if (isNext)
                _stateText.text = "NEXT";
            else if (isPassed)
                _stateText.text = "COMPLETED";
            else
                _stateText.text = "LOCKED";
        }

        if (_unlockRequirementText != null)
        {
            if (isCurrent)
            {
                if (currentIndex >= _locations.Length - 1)
                {
                    _unlockRequirementText.text = "Bạn đã đến địa điểm cuối cùng";
                }
                else
                {
                    int nextTarget = (currentIndex + 1) * _scorePerUnlock;
                    int remaining = Mathf.Max(0, nextTarget - highScore);
                    _unlockRequirementText.text = remaining + " điểm nữa để tới " + _locations[currentIndex + 1].locationName;
                }
            }
            else if (isNext)
            {
                int requiredScore = _currentPreviewIndex * _scorePerUnlock;
                int remaining = Mathf.Max(0, requiredScore - highScore);
                _unlockRequirementText.text = remaining + " điểm nữa để mở " + data.locationName;
            }
            else if (isPassed)
            {
                _unlockRequirementText.text = "Bạn đã đi qua địa điểm này";
            }
            else
            {
                int requiredScore = _currentPreviewIndex * _scorePerUnlock;
                _unlockRequirementText.text = "Cần " + requiredScore + " điểm để tới " + data.locationName;
            }
        }
    }

    public void ApplyProgressBackground()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int currentIndex = GetCurrentLocationIndex(highScore);

        if (_mainBackgroundRenderer != null &&
            currentIndex >= 0 &&
            currentIndex < _locations.Length)
        {
            _mainBackgroundRenderer.sprite = _locations[currentIndex].backgroundSprite;
        }
    }

    public void RefreshUnlockUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int currentIndex = GetCurrentLocationIndex(highScore);

        for (int i = 0; i < _locations.Length; i++)
        {
            bool isLocked = i > currentIndex;

            if (_locations[i].lockObject != null)
                _locations[i].lockObject.SetActive(isLocked);
        }

        if (_progressText != null)
        {
            if (currentIndex >= _locations.Length - 1)
            {
                _progressText.text = "Bạn đã đến " + _locations[currentIndex].locationName + "\nĐã mở toàn bộ hành trình";
            }
            else
            {
                int nextTarget = (currentIndex + 1) * _scorePerUnlock;
                int remaining = Mathf.Max(0, nextTarget - highScore);

                _progressText.text =
                    "Hiện tại: " + _locations[currentIndex].locationName +
                    "\n" + remaining + " điểm nữa để tới " + _locations[currentIndex + 1].locationName;
            }
        }
    }

    private int GetCurrentLocationIndex(int highScore)
    {
        int index = highScore / _scorePerUnlock;
        return Mathf.Clamp(index, 0, _locations.Length - 1);
    }

    private int GetNextLocationIndex(int highScore)
    {
        int current = GetCurrentLocationIndex(highScore);
        return Mathf.Clamp(current + 1, 0, _locations.Length - 1);
    }
}