using System.Collections.Generic;
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
        public int routeOrder; // HA NOI = 0, NGHE AN = 1, ...
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
    private Dictionary<int, int> _routeOrderToArrayIndex = new Dictionary<int, int>();

    private void Awake()
    {
        Instance = this;
        BuildRouteLookup();
    }

    private void Start()
    {
        BuildButtons();
        RefreshUnlockUI();
        ApplyProgressBackground();

        if (_backgroundPopup != null)
            _backgroundPopup.SetActive(false);
    }

    private void BuildRouteLookup()
    {
        _routeOrderToArrayIndex.Clear();

        for (int i = 0; i < _locations.Length; i++)
        {
            int order = _locations[i].routeOrder;

            if (!_routeOrderToArrayIndex.ContainsKey(order))
            {
                _routeOrderToArrayIndex.Add(order, i);
            }
            else
            {
                Debug.LogWarning("Duplicate routeOrder found: " + order + " on " + _locations[i].locationName);
            }
        }
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

        int currentRouteOrder = GetCurrentRouteOrder(highScore);
        int? nextRouteOrder = GetNextRouteOrder(highScore);

        bool isCurrent = data.routeOrder == currentRouteOrder;
        bool isNext = nextRouteOrder.HasValue && data.routeOrder == nextRouteOrder.Value;
        bool isPassed = data.routeOrder < currentRouteOrder;
        bool isLocked = data.routeOrder > currentRouteOrder;

        if (_previewImage != null)
            _previewImage.sprite = data.previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = ToUpperSafe(data.locationName);

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
                if (currentRouteOrder >= GetMaxRouteOrder())
                {
                    _unlockRequirementText.text = "FINAL LOCATION REACHED";
                }
                else
                {
                    int nextOrder = currentRouteOrder + 1;
                    int nextIndex = GetArrayIndexByRouteOrder(nextOrder);
                    int nextRequiredScore = GetRequiredScoreForRouteOrder(nextOrder);
                    int remaining = Mathf.Max(0, nextRequiredScore - highScore);

                    _unlockRequirementText.text =
                        remaining + " MORE POINTS\nTO REACH " +
                        ToUpperSafe(_locations[nextIndex].locationName);
                }
            }
            else if (isNext)
            {
                int requiredScore = GetRequiredScoreForRouteOrder(data.routeOrder);
                int remaining = Mathf.Max(0, requiredScore - highScore);

                _unlockRequirementText.text =
                    remaining + " MORE POINTS\nTO UNLOCK " +
                    ToUpperSafe(data.locationName);
            }
            else if (isPassed)
            {
                _unlockRequirementText.text = "LOCATION COMPLETED";
            }
            else
            {
                int requiredScore = GetRequiredScoreForRouteOrder(data.routeOrder);

                _unlockRequirementText.text =
                    requiredScore + " POINTS NEEDED\nTO REACH " +
                    ToUpperSafe(data.locationName);
            }
        }
    }

    public void ApplyProgressBackground()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int currentRouteOrder = GetCurrentRouteOrder(highScore);
        int currentIndex = GetArrayIndexByRouteOrder(currentRouteOrder);

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
        int currentRouteOrder = GetCurrentRouteOrder(highScore);

        for (int i = 0; i < _locations.Length; i++)
        {
            bool isLocked = _locations[i].routeOrder > currentRouteOrder;

            if (_locations[i].lockObject != null)
                _locations[i].lockObject.SetActive(isLocked);
        }

        if (_progressText != null)
        {
            int currentIndex = GetArrayIndexByRouteOrder(currentRouteOrder);

            if (currentRouteOrder >= GetMaxRouteOrder())
            {
                _progressText.text =
                    "CURRENT: " + ToUpperSafe(_locations[currentIndex].locationName) +
                    "\nJOURNEY COMPLETED";
            }
            else
            {
                int nextOrder = currentRouteOrder + 1;
                int nextIndex = GetArrayIndexByRouteOrder(nextOrder);
                int nextRequiredScore = GetRequiredScoreForRouteOrder(nextOrder);
                int remaining = Mathf.Max(0, nextRequiredScore - highScore);

                _progressText.text =
                    "CURRENT: " + ToUpperSafe(_locations[currentIndex].locationName) +
                    "\n" + remaining + " MORE POINTS TO REACH " +
                    ToUpperSafe(_locations[nextIndex].locationName);
            }
        }
    }

    private int GetCurrentRouteOrder(int highScore)
    {
        int order = highScore / _scorePerUnlock;
        return Mathf.Clamp(order, 0, GetMaxRouteOrder());
    }

    private int? GetNextRouteOrder(int highScore)
    {
        int currentOrder = GetCurrentRouteOrder(highScore);

        if (currentOrder >= GetMaxRouteOrder())
            return null;

        return currentOrder + 1;
    }

    private int GetRequiredScoreForRouteOrder(int routeOrder)
    {
        return Mathf.Max(0, routeOrder) * _scorePerUnlock;
    }

    private int GetArrayIndexByRouteOrder(int routeOrder)
    {
        if (_routeOrderToArrayIndex.TryGetValue(routeOrder, out int index))
            return index;

        Debug.LogError("No location found for routeOrder: " + routeOrder);
        return 0;
    }

    private int GetMaxRouteOrder()
    {
        return Mathf.Max(0, _locations.Length - 1);
    }

    private string ToUpperSafe(string value)
    {
        return string.IsNullOrEmpty(value) ? "" : value.ToUpper();
    }
}