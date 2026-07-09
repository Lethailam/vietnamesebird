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
        public Sprite previewSprite;      // ảnh hiện bên phải
        public Sprite backgroundSprite;   // background chính khi set
        public Button mapButton;          // button nằm trên bản đồ
        public GameObject lockObject;     // icon lock nếu có
    }

    [Header("Locations")]
    [SerializeField] private LocationData[] _locations;

    [Header("Right Preview UI")]
    [SerializeField] private Image _previewImage;
    [SerializeField] private TextMeshProUGUI _locationNameText;
    [SerializeField] private Button _setBackgroundButton;
    [SerializeField] private TextMeshProUGUI _setButtonText;

    [Header("Progress UI")]
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Game Background Target")]
    [SerializeField] private SpriteRenderer _mainBackgroundRenderer;

    [Header("Unlock Settings")]
    [SerializeField] private int _scorePerUnlock = 5;

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
        LoadSelectedBackground();
        ShowPreview(0);
    }

    private void BuildButtons()
    {
        for (int i = 0; i < _locations.Length; i++)
        {
            int index = i;

            if (_locations[i].mapButton != null)
            {
                _locations[i].mapButton.onClick.RemoveAllListeners();
                _locations[i].mapButton.onClick.AddListener(() => OnClickLocation(index));
            }
        }

        if (_setBackgroundButton != null)
        {
            _setBackgroundButton.onClick.RemoveAllListeners();
            _setBackgroundButton.onClick.AddListener(SetCurrentPreviewAsMainBackground);
        }
    }

    public void OnClickLocation(int index)
    {
        if (!IsLocationUnlocked(index))
            return;

        ShowPreview(index);
    }

    private void ShowPreview(int index)
    {
        if (index < 0 || index >= _locations.Length) return;

        _currentPreviewIndex = index;

        if (_previewImage != null)
            _previewImage.sprite = _locations[index].previewSprite;

        if (_locationNameText != null)
            _locationNameText.text = _locations[index].locationName;

        RefreshSetButtonState();
    }

    public void SetCurrentPreviewAsMainBackground()
    {
        if (!IsLocationUnlocked(_currentPreviewIndex))
            return;

        PlayerPrefs.SetInt(SELECTED_BG_KEY, _currentPreviewIndex);
        PlayerPrefs.Save();

        ApplySelectedBackgroundToGame();
        RefreshSetButtonState();
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

    private void LoadSelectedBackground()
    {
        ApplySelectedBackgroundToGame();
    }

    public void RefreshUnlockUI()
{
    int highScore = PlayerPrefs.GetInt("HighScore", 0);
    int unlockedCount = GetUnlockedLocationCount(highScore);

    for (int i = 0; i < _locations.Length; i++)
    {
        bool isUnlocked = i < unlockedCount;

        if (_locations[i].mapButton != null)
            _locations[i].mapButton.interactable = isUnlocked;

        if (_locations[i].lockObject != null)
            _locations[i].lockObject.SetActive(!isUnlocked);
    }

    if (_progressText != null)
    {
        int nextUnlockScore = GetNextUnlockScore(highScore);

        if (nextUnlockScore < 0)
        {
            _progressText.text = "Đã mở khóa tất cả địa điểm";
        }
        else
        {
            _progressText.text = "Điểm cao nhất: " + highScore +
                                 "\nMở khóa tiếp theo tại: " + nextUnlockScore;
        }
    }

    RefreshSetButtonState();
}

    private void RefreshSetButtonState()
    {
        if (_setBackgroundButton == null || _setButtonText == null) return;

        bool unlocked = IsLocationUnlocked(_currentPreviewIndex);
        int selectedIndex = PlayerPrefs.GetInt(SELECTED_BG_KEY, 0);
        bool isSelected = selectedIndex == _currentPreviewIndex;

        _setBackgroundButton.interactable = unlocked && !isSelected;

        if (!unlocked)
            _setButtonText.text = "LOCKED";
        else if (isSelected)
            _setButtonText.text = "SELECTED";
        else
            _setButtonText.text = "SET BACKGROUND";
    }

    private bool IsLocationUnlocked(int index)
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int unlockedCount = GetUnlockedLocationCount(highScore);
        return index >= 0 && index < unlockedCount;
    }

    private int GetUnlockedLocationCount(int highScore)
    {
        // Hà Nội mở sẵn
        // mỗi 100 điểm mở thêm 1 địa điểm
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