using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyLevelVisuals();
        UpdateLevelUI();
    }

    public void RegisterPipePassed()
    {
        _pipesPassedInCurrentLevel++;

        if (_pipesPassedInCurrentLevel >= _pipesToNextLevel)
        {
            NextLevel();
        }
    }

    public void ContinueToNextLevel()
    {
    Time.timeScale = 1f;

    _currentLevel++;
    _pipesPassedInCurrentLevel = 0;

    if (_currentLevel > _backgrounds.Length)
    {
        _currentLevel = _backgrounds.Length;
    }

    ApplyLevelVisuals();
    UpdateLevelUI();

    if (BirdShopManager.Instance != null)
    {
        BirdShopManager.Instance.HidePanel();
    }
    }

    private void NextLevel()
    {
        if (BirdShopManager.Instance != null)
        {
            BirdShopManager.Instance.ShowBirdOfferForLevel(_currentLevel);
        }

        Time.timeScale = 0f;
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
    }

    private void UpdateLevelUI()
    {
        if (_levelText != null)
        {
            _levelText.text = "Level " + _currentLevel;
        }
    }
}
