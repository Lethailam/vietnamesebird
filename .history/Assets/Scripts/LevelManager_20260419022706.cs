using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    [SerializeField] private int _pipesToNextLevel = 5;
    [SerializeField] private int _currentLevel = 1;
    [SerializeField] private int _pipesPassedInCurrentLevel = 0;

    [Header("Scene References")]
    [SerializeField] private SpriteRenderer _backgroundRenderer;
    [SerializeField] private SpriteRenderer _groundRenderer;
    [SerializeField] private PipeSpawner _pipeSpawner;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Level Assets")]
    [SerializeField] private Sprite[] _backgrounds;
    [SerializeField] private Sprite[] _grounds;
    [SerializeField] private GameObject[] _pipePrefabs;

    private const string CURRENT_LEVEL_KEY = "CURRENT_LEVEL";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 1);
        ApplyLevelVisuals();
        UpdateLevelUI();
    }

    public void RegisterPipePassed()
    {
        _pipesPassedInCurrentLevel++;

        if (_pipesPassedInCurrentLevel >= _pipesToNextLevel)
        {
            ContinueToNextLevel();
        }
    }

    public void ContinueToNextLevel()
    {
        _currentLevel++;
        _pipesPassedInCurrentLevel = 0;

        int maxLevel = Mathf.Min(_backgrounds.Length, Mathf.Min(_grounds.Length, _pipePrefabs.Length));
        if (_currentLevel > maxLevel)
        {
            _currentLevel = maxLevel;
        }

        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, _currentLevel);
        PlayerPrefs.Save();

        ApplyLevelVisuals();
        UpdateLevelUI();

        if (BackgroundMapManager.Instance != null)
        {
            BackgroundMapManager.Instance.RefreshUnlockUI();
        }
    }

    private void ApplyLevelVisuals()
    {
        int index = _currentLevel - 1;

        if (_backgroundRenderer != null && index < _backgrounds.Length)
            _backgroundRenderer.sprite = _backgrounds[index];

        if (_groundRenderer != null && index < _grounds.Length)
            _groundRenderer.sprite = _grounds[index];

        if (_pipeSpawner != null && index < _pipePrefabs.Length)
            _pipeSpawner.SetPipePrefab(_pipePrefabs[index]);

        if (_groundRenderer != null)
        {
            LoopGround loopGround = _groundRenderer.GetComponent<LoopGround>();
            if (loopGround != null)
            {
                loopGround.RefreshVisualBounds();
            }
        }
    }

    private void UpdateLevelUI()
    {
        if (_levelText != null)
        {
            _levelText.text = "LEVEL " + _currentLevel;
        }
    }

    public int GetCurrentLevel()
    {
        return _currentLevel;
    }

    public int GetMaxLevel()
    {
        return _backgrounds != null ? _backgrounds.Length : 0;
    }
}