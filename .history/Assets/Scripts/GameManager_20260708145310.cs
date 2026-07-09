using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameOverCanvas;
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _mapPanel;

    [Header("Gameplay")]
    [SerializeField] private PipeSpawner _pipeSpawner;
    [SerializeField] private FlyBehavior _flyBehavior;
    [SerializeField] private GameObject _gameplayHUD;

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

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
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

        // Hiện Banner ở Main Menu.
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

        // Banner tiếp tục hiện trong lúc chim bay.
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

        // Không hiện Banner trong Shop.
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

        // Không hiện Banner trong Shop.
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

            // Trở lại Main Menu nên hiện Banner.
            ShowBanner();
        }
        else
        {
            if (_startPanel != null)
                _startPanel.SetActive(false);

            if (_gameOverCanvas != null)
                _gameOverCanvas.SetActive(true);

            // Trở lại Game Over nên Banner vẫn ẩn.
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

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        // Trở lại Main Menu.
        ShowBanner();
    }

    public void BackToGameOverFromShop()
    {
        Time.timeScale = 0f;

        if (_shopPanel != null)
            _shopPanel.SetActive(false);

        if (_startPanel != null)
            _startPanel.SetActive(false);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(true);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        // Game Over không hiện Banner.
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

        // Không hiện Banner trong Map.
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

        if (_startPanel != null)
            _startPanel.SetActive(true);

        if (_gameOverCanvas != null)
            _gameOverCanvas.SetActive(false);

        if (_gameplayHUD != null)
            _gameplayHUD.SetActive(false);

        // Trở lại Main Menu.
        ShowBanner();
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    public void GameOver()
    {
        if (!_gameStarted)
        {
            return;
        }

        _gameStarted = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        if (_gameOverCanvas != null)
        {
            _gameOverCanvas.SetActive(true);
        }

        if (_gameplayHUD != null)
        {
            _gameplayHUD.SetActive(false);
        }

        Time.timeScale = 0f;

        if (AdsManager.Instance != null)
        {
            // Game Over không hiện Banner.
            AdsManager.Instance.HideBanner();

            // Hiện quảng cáo Interstitial.
            AdsManager.Instance.ShowGameOverAd();
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy AdsManager trong Scene."
            );
        }
    }

    // =========================================================
    // ADS HELPERS
    // =========================================================

    private void ShowBanner()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowBanner();
        }
    }

    private void HideBanner()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.HideBanner();
        }
    }

    // Giữ tên cũ để không làm mất liên kết Button trong Inspector.
    public void RestarGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}