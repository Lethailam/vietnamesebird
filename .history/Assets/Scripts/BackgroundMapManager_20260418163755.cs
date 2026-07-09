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
        "HANOI",
        "NGHEAN",
        "DONGHOI",
        "DONGHA",
        "HUE",
        "DANANG",
        "NHATRANG",
        "SAIGON"
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

        if (_previewImage != null)
            _previewImage.sprite = data.previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = DisplayName(data.locationName);

        if (clickedStep < 0)
        {
            if (_dimOverlay != null)
                _dimOverlay.SetActive(true);

            if (_stateText != null)
                _stateText.text = "INVALID";

            if (_unlockRequirementText != null)
                _unlockRequirementText.text = "LOCATION NOT IN ROUTE";

            return;
        }

        bool isCurrent = clickedStep == currentStep;
        bool isNext = clickedStep == currentStep + 1 && currentStep < ROUTE_ORDER.Length - 1;
        bool isPassed = clickedStep < currentStep;
        bool isLocked = clickedStep > currentStep;

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
            if (isCurrent)
{
    if (currentStep == 0)
    {
        _unlockRequirementText.text = "";
    }
    else if (currentStep >= ROUTE_ORDER.Length - 1)
    {
        _unlockRequirementText.text = "FINAL LOCATION REACHED";
    }
    else
    {
        int nextRequiredScore = GetRequiredScoreForStep(currentStep + 1);
        _unlockRequirementText.text =
            "GAIN " + nextRequiredScore + " POINTS\nTO REACH " +
            DisplayName(ROUTE_ORDER[currentStep + 1]);
    }
}
            else if (isNext)
            {
                int requiredScore = GetRequiredScoreForStep(clickedStep);
                _unlockRequirementText.text =
                    "GAIN " + requiredScore + " POINTS\nTO REACH " +
                    DisplayName(data.locationName);
            }
            else if (isPassed)
            {
                _unlockRequirementText.text = "LOCATION COMPLETED";
            }
            else
            {
                int requiredScore = GetRequiredScoreForStep(clickedStep);
                _unlockRequirementText.text =
                    "GAIN " + requiredScore + " POINTS\nTO REACH " +
                    DisplayName(data.locationName);
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
                    "CURRENT: " + DisplayName(ROUTE_ORDER[currentStep]) +
                    "\nJOURNEY COMPLETED";
            }
            else
            {
                int nextRequiredScore = GetRequiredScoreForStep(currentStep + 1);

                _progressText.text =
                    "CURRENT: " + DisplayName(ROUTE_ORDER[currentStep]) +
                    "\nGAIN " + nextRequiredScore + " POINTS TO REACH " +
                    DisplayName(ROUTE_ORDER[currentStep + 1]);
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
        string normalized = CanonicalName(locationName);

        for (int i = 0; i < ROUTE_ORDER.Length; i++)
        {
            if (ROUTE_ORDER[i] == normalized)
                return i;
        }

        return -1;
    }

    private int GetArrayIndexByRouteName(string routeName)
    {
        string normalizedRoute = CanonicalName(routeName);

        for (int i = 0; i < _locations.Length; i++)
        {
            if (CanonicalName(_locations[i].locationName) == normalizedRoute)
                return i;
        }

        Debug.LogError("NO LOCATION FOUND FOR NAME: " + normalizedRoute);
        return -1;
    }

    private string CanonicalName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        string s = value.Trim().ToUpper();

        s = s.Replace(" ", "");
        s = s.Replace("_", "");
        s = s.Replace("-", "");

        s = s.Replace("À", "A").Replace("Á", "A").Replace("Ạ", "A").Replace("Ả", "A").Replace("Ã", "A");
        s = s.Replace("Â", "A").Replace("Ầ", "A").Replace("Ấ", "A").Replace("Ậ", "A").Replace("Ẩ", "A").Replace("Ẫ", "A");
        s = s.Replace("Ă", "A").Replace("Ằ", "A").Replace("Ắ", "A").Replace("Ặ", "A").Replace("Ẳ", "A").Replace("Ẵ", "A");
        s = s.Replace("È", "E").Replace("É", "E").Replace("Ẹ", "E").Replace("Ẻ", "E").Replace("Ẽ", "E");
        s = s.Replace("Ê", "E").Replace("Ề", "E").Replace("Ế", "E").Replace("Ệ", "E").Replace("Ể", "E").Replace("Ễ", "E");
        s = s.Replace("Ì", "I").Replace("Í", "I").Replace("Ị", "I").Replace("Ỉ", "I").Replace("Ĩ", "I");
        s = s.Replace("Ò", "O").Replace("Ó", "O").Replace("Ọ", "O").Replace("Ỏ", "O").Replace("Õ", "O");
        s = s.Replace("Ô", "O").Replace("Ồ", "O").Replace("Ố", "O").Replace("Ộ", "O").Replace("Ổ", "O").Replace("Ỗ", "O");
        s = s.Replace("Ơ", "O").Replace("Ờ", "O").Replace("Ớ", "O").Replace("Ợ", "O").Replace("Ở", "O").Replace("Ỡ", "O");
        s = s.Replace("Ù", "U").Replace("Ú", "U").Replace("Ụ", "U").Replace("Ủ", "U").Replace("Ũ", "U");
        s = s.Replace("Ư", "U").Replace("Ừ", "U").Replace("Ứ", "U").Replace("Ự", "U").Replace("Ử", "U").Replace("Ữ", "U");
        s = s.Replace("Ỳ", "Y").Replace("Ý", "Y").Replace("Ỵ", "Y").Replace("Ỷ", "Y").Replace("Ỹ", "Y");
        s = s.Replace("Đ", "D");

        if (s == "HCM" || s == "HOCHIMINH" || s == "HOCHIMINHCITY")
            s = "SAIGON";

        return s;
    }

    private string DisplayName(string value)
    {
        string canonical = CanonicalName(value);

        switch (canonical)
        {
            case "HANOI": return "HA NOI";
            case "NGHEAN": return "NGHE AN";
            case "DONGHOI": return "DONG HOI";
            case "DONGHA": return "DONG HA";
            case "HUE": return "HUE";
            case "DANANG": return "DA NANG";
            case "NHATRANG": return "NHA TRANG";
            case "SAIGON": return "HCM";
            default: return string.IsNullOrWhiteSpace(value) ? "" : value.Trim().ToUpper();
        }
    }
}