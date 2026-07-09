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

    private static readonly string[] ROUTE_ORDER =
    {
        "HA NOI",
        "NGHE AN",
        "DONG HOI",
        "DONG HA",
        "HUE",
        "DA NANG",
        "NHA TRANG",
        "SAI GON"
    };

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

        int currentStep = GetCurrentStep(highScore);
        int clickedStep = GetStepOfLocation(data.locationName);

        bool isCurrent = clickedStep == currentStep;
        bool isNext = clickedStep == currentStep + 1 && currentStep < ROUTE_ORDER.Length - 1;
        bool isPassed = clickedStep < currentStep;
        bool isLocked = clickedStep > currentStep;

        if (_previewImage != null)
            _previewImage.sprite = data.previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = NormalizeName(data.locationName);

        if (_dimOverlay != null)
            _dimOverlay.SetActive(isLocked);

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
            if (clickedStep < 0)
            {
                _unlockRequirementText.text = "LOCATION NOT IN ROUTE";
            }
            else if (isCurrent)
            {
                if (currentStep >= ROUTE_ORDER.Length - 1)
                {
                    _unlockRequirementText.text = "FINAL LOCATION REACHED";
                }
                else
                {
                    int nextRequiredScore = GetRequiredScoreForStep(currentStep + 1);
                    int remaining = Mathf.Max(0, nextRequiredScore - highScore);

                    _unlockRequirementText.text =
                        remaining + " MORE POINTS\nTO REACH " +
                        ROUTE_ORDER[currentStep + 1];
                }
            }
            else if (isNext)
            {
                int requiredScore = GetRequiredScoreForStep(clickedStep);
                int remaining = Mathf.Max(0, requiredScore - highScore);

                _unlockRequirementText.text =
                    remaining + " MORE POINTS\nTO UNLOCK " +
                    NormalizeName(data.locationName);
            }
            else if (isPassed)
            {
                _unlockRequirementText.text = "LOCATION COMPLETED";
            }
            else
            {
                int requiredScore = GetRequiredScoreForStep(clickedStep);

                _unlockRequirementText.text =
                    requiredScore + " POINTS NEEDED\nTO REACH " +
                    NormalizeName(data.locationName);
            }
        }
    }

    public void ApplyProgressBackground()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int currentStep = GetCurrentStep(highScore);
        int currentIndex = GetArrayIndexByRouteName(ROUTE_ORDER[currentStep]);

        if (currentIndex >= 0 &&
            currentIndex < _locations.Length &&
            _mainBackgroundRenderer != null)
        {
            _mainBackgroundRenderer.sprite = _locations[currentIndex].backgroundSprite;
        }
    }

    public void RefreshUnlockUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int currentStep = GetCurrentStep(highScore);

        for (int i = 0; i < _locations.Length; i++)
        {
            int step = GetStepOfLocation(_locations[i].locationName);
            bool isLocked = step > currentStep;

            if (_locations[i].lockObject != null)
                _locations[i].lockObject.SetActive(isLocked);
        }

        if (_progressText != null)
        {
            if (currentStep >= ROUTE_ORDER.Length - 1)
            {
                _progressText.text =
                    "CURRENT: " + ROUTE_ORDER[currentStep] +
                    "\nJOURNEY COMPLETED";
            }
            else
            {
                int nextRequiredScore = GetRequiredScoreForStep(currentStep + 1);
                int remaining = Mathf.Max(0, nextRequiredScore - highScore);

                _progressText.text =
                    "CURRENT: " + ROUTE_ORDER[currentStep] +
                    "\n" + remaining + " MORE POINTS TO REACH " +
                    ROUTE_ORDER[currentStep + 1];
            }
        }
    }

    private int GetCurrentStep(int highScore)
    {
        int step = highScore / _scorePerUnlock;
        return Mathf.Clamp(step, 0, ROUTE_ORDER.Length - 1);
    }

    private int GetRequiredScoreForStep(int step)
    {
        return Mathf.Max(0, step) * _scorePerUnlock;
    }

    private int GetStepOfLocation(string locationName)
    {
        string normalized = NormalizeName(locationName);

        for (int i = 0; i < ROUTE_ORDER.Length; i++)
        {
            if (ROUTE_ORDER[i] == normalized)
                return i;
        }

        return -1;
    }

    private int GetArrayIndexByRouteName(string routeName)
    {
        string normalizedRoute = NormalizeName(routeName);

        for (int i = 0; i < _locations.Length; i++)
        {
            if (NormalizeName(_locations[i].locationName) == normalizedRoute)
                return i;
        }

        Debug.LogError("NO LOCATION FOUND FOR NAME: " + normalizedRoute);
        return -1;
    }

    private string NormalizeName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "" : value.Trim().ToUpper();
    }
}