using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameOverCanvas;
    [SerializeField] private GameObject _shopPanel;

    [Header("Gameplay")]
    [SerializeField] private PipeSpawner _pipeSpawner;
    [SerializeField] private FlyBehavior _flyBehavior;
    [SerializeField] private GameObject _gameplayHUD;

    [SerializeField] private GameObject _mapPanel;

    private bool _gameStarted = false;

    private static bool _playImmediatelyAfterReload = false;

    private enum ShopOpenSource
{
    Start,
    GameOver
}

private ShopOpenSource _shopOpenSource = ShopOpenSource.Start;

    private void Awake()
{
    LockPortraitScreen();

    if (instance == null)
    {
        instance = this;
    }
}

    private void LockPortraitScreen()
{
    Screen.autorotateToPortrait = true;
    Screen.autorotateToPortraitUpsideDown = false;
    Screen.autorotateToLandscapeLeft = false;
    Screen.autorotateToLandscapeRight = false;

    Screen.orientation = ScreenOrientation.Portrait;
}

    private void Start()
{
    if (_playImmediatelyAfterReload)
    {
        _playImmediatelyAfterReload = false;
        StartGame();
    }
    else
    {
        ShowStartScreen();
    }
}

    public void ShowStartScreen()
{
    _gameStarted = false;
    Time.timeScale = 0f;

    if (_startPanel != null) _startPanel.SetActive(true);
    if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    if (_shopPanel != null) _shopPanel.SetActive(false);
    if (_mapPanel != null) _mapPanel.SetActive(false);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);
}

public void PlayAgain()
{
    _playImmediatelyAfterReload = true;
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

    public void StartGame()
{
    _gameStarted = true;
    Time.timeScale = 1f;

    if (_startPanel != null) _startPanel.SetActive(false);
    if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    if (_shopPanel != null) _shopPanel.SetActive(false);
    if (_mapPanel != null) _mapPanel.SetActive(false);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(true);
}

    public void OpenShopFromMenu()
{
    _shopOpenSource = ShopOpenSource.Start;
    Time.timeScale = 0f;

    if (_startPanel != null) _startPanel.SetActive(false);
    if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    if (_shopPanel != null) _shopPanel.SetActive(true);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);

    if (ShopGridManager.Instance != null)
    {
        ShopGridManager.Instance.OpenBirdShop();
    }
}

public void OpenShopFromGameOver()
{
    _shopOpenSource = ShopOpenSource.GameOver;
    Time.timeScale = 0f;

    if (_startPanel != null) _startPanel.SetActive(false);
    if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    if (_shopPanel != null) _shopPanel.SetActive(true);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);

    if (ShopGridManager.Instance != null)
    {
        ShopGridManager.Instance.OpenBirdShop();
    }
}

public void BackFromShop()
{
    Time.timeScale = 0f;

    if (_shopPanel != null) _shopPanel.SetActive(false);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);

    if (_shopOpenSource == ShopOpenSource.Start)
    {
        if (_startPanel != null) _startPanel.SetActive(true);
        if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    }
    else if (_shopOpenSource == ShopOpenSource.GameOver)
    {
        if (_startPanel != null) _startPanel.SetActive(false);
        if (_gameOverCanvas != null) _gameOverCanvas.SetActive(true);
    }
}

public void OpenMapFromMenu()
{
    Time.timeScale = 0f;

    if (_startPanel != null) _startPanel.SetActive(false);
    if (_shopPanel != null) _shopPanel.SetActive(false);
    if (_gameOverCanvas != null) _gameOverCanvas.SetActive(false);
    if (_mapPanel != null) _mapPanel.SetActive(true);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);

    if (BackgroundMapManager.Instance != null)
    {
        BackgroundMapManager.Instance.RefreshUnlockUI();
    }
}

public void BackToStartFromMap()
{
    Time.timeScale = 0f;

    if (_mapPanel != null) _mapPanel.SetActive(false);
    if (_startPanel != null) _startPanel.SetActive(true);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);
}

    public void BackToStartFromShop()
{
    Time.timeScale = 0f;

    if (_shopPanel != null) _shopPanel.SetActive(false);
    if (_startPanel != null) _startPanel.SetActive(true);
    if (_gameplayHUD != null) _gameplayHUD.SetActive(false);
}

    public void BackToGameOverFromShop()
    {
        Time.timeScale = 0f;

        if (_shopPanel != null) _shopPanel.SetActive(false);
        if (_gameOverCanvas != null) _gameOverCanvas.SetActive(true);
    }

    public void GameOver()
{
    if (!_gameStarted) return;

    _gameStarted = false;

    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayGameOver();
    }

    if (_gameOverCanvas != null)
    {
        _gameOverCanvas.SetActive(true);
    }

    Time.timeScale = 0f;
}

    public void RestarGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}