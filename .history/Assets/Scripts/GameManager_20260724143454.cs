using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField]
    private GameObject _startPanel;

    [SerializeField]
    private GameObject _gameOverCanvas;

    [SerializeField]
    private GameObject _shopPanel;

    [SerializeField]
    private GameObject _mapPanel;

    [Header("Gameplay")]
    [SerializeField]
    private PipeSpawner _pipeSpawner;

    [SerializeField]
    private FlyBehavior _flyBehavior;

    [Header("Ads Delay")]
    [SerializeField] private float _gameOverAdDelay = 0.8f;

    private Coroutine _showAdCoroutine;

    [SerializeField]
    private GameObject _gameplayHUD;

    private bool _gameStarted;

    private static bool _playImmediatelyAfterReload;

    private enum ShopOpenSource
    {
        Start,
        GameOver
    }

    private ShopOpenSource _shopOpenSource =
        ShopOpenSource.Start;

    private void Awake()
    {
        LockPortraitScreen();

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void LockPortraitScreen()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;

        Screen.orientation =
            ScreenOrientation.Portrait;
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

    // =========================================================
    // MAIN MENU
    // =========================================================

    public void ShowStartScreen()
    {
        _gameStarted = false;
        Time.timeScale = 0f;

        if (_startPanel != null)
            _startPanel.SetActive(true);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        // Banner phải hiện tại Main Menu.
        ShowBanner();
    }

    // =========================================================
    // GAMEPLAY
    // =========================================================

    public void StartGame()
    {
        _gameStarted = true;
        Time.timeScale = 1f;

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(true);

        // Banner vẫn hiện trong khi chim bay.
        ShowBanner();
    }

    public void PlayAgain()
    {
        _playImmediatelyAfterReload = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // SHOP
    // =========================================================

    public void OpenShopFromMenu()
    {
        _shopOpenSource = ShopOpenSource.Start;
        Time.timeScale = 0f;

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(true);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        HideBanner();

        if (ShopGridManager.Instance != null)
        {
            ShopGridManager.Instance.OpenBirdShop();
        }
    }

    public void OpenShopFromGameOver()
    {
        _shopOpenSource = ShopOpenSource.GameOver;
        Time.timeScale = 0f;

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(true);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        HideBanner();

        if (ShopGridManager.Instance != null)
        {
            ShopGridManager.Instance.OpenBirdShop();
        }
    }

    public void BackFromShop()
    {
        Time.timeScale = 0f;

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        if (_shopOpenSource == ShopOpenSource.Start)
        {
            if (_startPanel != null)
                _startPanel.SetActive(true);

            if (_gameOverCanvas != null)
                _gameOverCanvas.SetActive(false);

            ShowBanner();
        }
        else
        {
            if (_startPanel != null)
                _startPanel.SetActive(false);

            if (_gameOverCanvas != null)
                _gameOverCanvas.SetActive(true);

            HideBanner();
        }
    }

    public void BackToStartFromShop()
    {
        Time.timeScale = 0f;

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_startPanel != null)
            _startPanel.SetActive(true);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        ShowBanner();
    }

    public void BackToGameOverFromShop()
    {
        Time.timeScale = 0f;

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(true);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        HideBanner();
    }

    // =========================================================
    // MAP
    // =========================================================

    public void OpenMapFromMenu()
    {
        Time.timeScale = 0f;

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_mapPanel != null)
            _mapPanel.SetActive(true);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        HideBanner();

        if (BackgroundMapManager.Instance != null)
        {
            BackgroundMapManager.Instance.RefreshUnlockUI();
        }
    }

    public void BackToStartFromMap()
    {
        Time.timeScale = 0f;

        if (_mapPanel != null)
            _mapPanel.SetActive(false);

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_startPanel != null)
            _startPanel.SetActive(true);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        ShowBanner();
    }

    // =========================================================
    // GAME OVER
    // =========================================================

            }


    // =========================================================
    // ADS HELPERS
    // =========================================================

    private void ShowBanner()
    {
        Debug.Log(
            "GAME MANAGER: Yêu cầu hiển thị Banner."
        );

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowBanner();
        }
        else
        {
            Debug.LogWarning(
                "GAME MANAGER: AdsManager chưa sẵn sàng."
            );
        }
    }

    private void HideBanner()
    {
        Debug.Log(
            "GAME MANAGER: Yêu cầu ẩn Banner."
        );

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.HideBanner();
        }
    }

    // Giữ đúng tên cũ để không mất liên kết Button trong Inspector.
    public void RestarGame()
    {
        PlayAgain();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}