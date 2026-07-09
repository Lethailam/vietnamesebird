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

        int currentStep = GetCurrentStepByLevel();
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
                    int nextLevel = currentStep + 2;
                    _unlockRequirementText.text =
                        "REACH LEVEL " + nextLevel + "\nTO UNLOCK " +
                        DisplayName(ROUTE_ORDER[currentStep + 1]);
                }
            }
            else if (isNext)
            {
                int requiredLevel = clickedStep + 1;
                _unlockRequirementText.text =
                    "REACH LEVEL " + requiredLevel + "\nTO UNLOCK " +
                    DisplayName(data.locationName);
            }
            else if (isPassed)
            {
                _unlockRequirementText.text = "LOCATION COMPLETED";
            }
            else
            {
                int requiredLevel = clickedStep + 1;
                _unlockRequirementText.text =
                    "REACH LEVEL " + requiredLevel + "\nTO UNLOCK " +
                    DisplayName(data.locationName);
            }
        }
    }

    public void RefreshUnlockUI()
    {
        int currentStep = GetCurrentStepByLevel();

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
                int nextLevel = currentStep + 2;

                _progressText.text =
                    "CURRENT: " + DisplayName(ROUTE_ORDER[currentStep]) +
                    "\nREACH LEVEL " + nextLevel + " TO UNLOCK " +
                    DisplayName(ROUTE_ORDER[currentStep + 1]);
            }
        }
    }

    private int GetCurrentStepByLevel()
    {
        if (LevelManager.Instance == null)
            return 0;

        int level = LevelManager.Instance.GetCurrentLevel();
        return Mathf.Clamp(level - 1, 0, ROUTE_ORDER.Length - 1);
    }

    private int GetStepOfLocation(string locationName)
    {
        string normalized = CanonicalName(locationName);

        for (int i = 0; i < ROUTE_ORDER.Length; i++)
        {
            if (CanonicalName(ROUTE_ORDER[i]) == normalized)
                return i;
        }

        return -1;
    }

    private string CanonicalName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        string s = value.Trim().ToUpper();
        s = s.Replace(" ", "").Replace("_", "").Replace("-", "");

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
            default: return value.Trim().ToUpper();
        }
    }
}