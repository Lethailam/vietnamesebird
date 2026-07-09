using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Test Mode")]
    [Tooltip("Luôn bật khi đang kiểm tra. Chỉ tắt trước khi phát hành.")]
    [SerializeField] private bool _useTestAds = true;

    [Header("Android Ad Unit IDs")]
    [Tooltip("Interstitial Ad Unit ID thật.")]
    [SerializeField] private string _androidInterstitialAdUnitId;

    [Tooltip("Banner Ad Unit ID thật.")]
    [SerializeField] private string _androidBannerAdUnitId;

    [Header("Game Over Ad Frequency")]
    [Tooltip("Số lần thua tối thiểu trước khi hiện quảng cáo.")]
    [SerializeField, Min(1)]
    private int _minGameOversBeforeAd = 3;

    [Tooltip("Số lần thua tối đa trước khi hiện quảng cáo.")]
    [SerializeField, Min(1)]
    private int _maxGameOversBeforeAd = 4;

    // ID quảng cáo thử nghiệm chính thức của Google cho Android.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string TestBannerId =
        "ca-app-pub-3940256099942544/6300978111";

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

    private bool _sdkInitialized;
    private bool _interstitialIsLoading;

    // Banner mặc định được phép hiện ở Main Menu.
    private bool _bannerShouldBeVisible = true;

    // Bộ đếm số lần Game Over.
    private int _gameOverCount;

    // Mốc Game Over tiếp theo để hiện quảng cáo: 3 hoặc 4.
    private int _nextAdGameOverCount;

    private string CurrentInterstitialId
    {
        get
        {
            return _useTestAds
                ? TestInterstitialId
                : _androidInterstitialAdUnitId;
        }
    }

    private string CurrentBannerId
    {
        get
        {
            return _useTestAds
                ? TestBannerId
                : _androidBannerAdUnitId;
        }
    }

    private void Awake()
    {
        // Chỉ giữ lại một AdsManager khi Scene reload.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PrepareNextInterstitialFrequency();
    }

    private void Start()
    {
        InitializeAds();
    }

    // =========================================================
    // KHỞI TẠO ADMOB
    // =========================================================

    private void InitializeAds()
    {
        if (_sdkInitialized)
        {
            return;
        }

        Debug.Log("ADMOB: Bắt đầu khởi tạo SDK.");

        // Đưa các callback quảng cáo về Unity Main Thread.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize(initializationStatus =>
        {
            if (initializationStatus == null)
            {
                Debug.LogError(
                    "ADMOB: Khởi tạo SDK thất bại."
                );

                return;
            }

            _sdkInitialized = true;

            Debug.Log(
                "ADMOB: Khởi tạo SDK thành công."
            );

            LoadBanner();
            LoadInterstitial();
        });
    }

    // =========================================================
    // BANNER
    // =========================================================

    private void LoadBanner()
    {
        if (!_sdkInitialized)
        {
            Debug.LogWarning(
                "BANNER: SDK chưa khởi tạo."
            );

            return;
        }

        DestroyBanner();

        if (string.IsNullOrWhiteSpace(CurrentBannerId))
        {
            Debug.LogError(
                "BANNER: Banner Ad Unit ID đang bị trống."
            );

            return;
        }

        Debug.Log(
            "BANNER: Bắt đầu tải."
        );

        // Banner cố định 320 x 50 ở cạnh dưới.
        _bannerView = new BannerView(
            CurrentBannerId,
            AdSize.Banner,
            AdPosition.Bottom
        );

        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log(
                "BANNER: Tải thành công."
            );

            if (_bannerShouldBeVisible)
            {
                _bannerView.Show();

                Debug.Log(
                    "BANNER: Đang hiển thị ở cạnh dưới."
                );
            }
            else
            {
                _bannerView.Hide();

                Debug.Log(
                    "BANNER: Đã tải nhưng đang được yêu cầu ẩn."
                );
            }
        };

        _bannerView.OnBannerAdLoadFailed += error =>
        {
            Debug.LogError(
                "BANNER: Tải thất bại.\n" +
                "Message: " +
                error.GetMessage() +
                "\nChi tiết: " +
                error
            );
        };

        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log(
                "BANNER: Đã ghi nhận lượt hiển thị."
            );
        };

        _bannerView.OnAdClicked += () =>
        {
            Debug.Log(
                "BANNER: Người dùng đã nhấn quảng cáo."
            );
        };

        _bannerView.LoadAd(new AdRequest());
    }

    public void ShowBanner()
    {
        _bannerShouldBeVisible = true;

        Debug.Log(
            "BANNER: GameManager yêu cầu hiển thị."
        );

        if (!_sdkInitialized)
        {
            Debug.LogWarning(
                "BANNER: SDK chưa khởi tạo. " +
                "Banner sẽ hiện sau khi tải xong."
            );

            return;
        }

        if (_bannerView == null)
        {
            Debug.LogWarning(
                "BANNER: Chưa có BannerView. " +
                "Tiến hành tải lại."
            );

            LoadBanner();
            return;
        }

        _bannerView.SetPosition(AdPosition.Bottom);
        _bannerView.Show();
    }

    public void HideBanner()
    {
        _bannerShouldBeVisible = false;

        Debug.Log(
            "BANNER: GameManager yêu cầu ẩn."
        );

        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
    }

    public void ReloadBanner()
    {
        Debug.Log(
            "BANNER: Yêu cầu tải lại thủ công."
        );

        LoadBanner();
    }

    private void DestroyBanner()
    {
        if (_bannerView == null)
        {
            return;
        }

        _bannerView.Destroy();
        _bannerView = null;
    }

    // =========================================================
    // TẦN SUẤT INTERSTITIAL
    // =========================================================

    private void PrepareNextInterstitialFrequency()
    {
        int minimum = Mathf.Max(
            1,
            _minGameOversBeforeAd
        );

        int maximum = Mathf.Max(
            minimum,
            _maxGameOversBeforeAd
        );

        _gameOverCount = 0;

        // Random.Range với int không lấy giá trị cuối,
        // nên cần maximum + 1.
        _nextAdGameOverCount = Random.Range(
            minimum,
            maximum + 1
        );

        Debug.Log(
            "INTERSTITIAL: Quảng cáo tiếp theo sẽ hiện sau " +
            _nextAdGameOverCount +
            " lần Game Over."
        );
    }

    // =========================================================
    // TẢI INTERSTITIAL
    // =========================================================

    private void LoadInterstitial()
    {
        if (!_sdkInitialized)
        {
            Debug.LogWarning(
                "INTERSTITIAL: SDK chưa khởi tạo."
            );

            return;
        }

        if (_interstitialIsLoading)
        {
            Debug.Log(
                "INTERSTITIAL: Quảng cáo đang được tải."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentInterstitialId))
        {
            Debug.LogError(
                "INTERSTITIAL: Ad Unit ID đang bị trống."
            );

            return;
        }

        DestroyInterstitial();

        _interstitialIsLoading = true;

        Debug.Log(
            "INTERSTITIAL: Bắt đầu tải."
        );

        InterstitialAd.Load(
            CurrentInterstitialId,
            new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                _interstitialIsLoading = false;

                if (error != null || ad == null)
                {
                    Debug.LogError(
                        "INTERSTITIAL: Tải thất bại.\n" +
                        error
                    );

                    return;
                }

                _interstitialAd = ad;

                Debug.Log(
                    "INTERSTITIAL: Tải thành công."
                );

                RegisterInterstitialEvents(ad);
            }
        );
    }

    private void RegisterInterstitialEvents(
        InterstitialAd ad
    )
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log(
                "INTERSTITIAL: Quảng cáo đã mở."
            );
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log(
                "INTERSTITIAL: Người chơi đã đóng quảng cáo."
            );

            DestroyInterstitial();

            // Tải quảng cáo mới cho lần tiếp theo.
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError(
                "INTERSTITIAL: Không thể mở quảng cáo.\n" +
                error.GetMessage()
            );

            DestroyInterstitial();

            // Tải lại quảng cáo mới.
            LoadInterstitial();
        };
    }

    // =========================================================
    // GỌI KHI GAME OVER
    // =========================================================

    public void ShowGameOverAd()
    {
        _gameOverCount++;

        Debug.Log(
            "INTERSTITIAL: Game Over " +
            _gameOverCount +
            "/" +
            _nextAdGameOverCount
        );

        // Chưa đủ 3 hoặc 4 lần thua.
        if (_gameOverCount < _nextAdGameOverCount)
        {
            Debug.Log(
                "INTERSTITIAL: Chưa đủ số lần thua. " +
                "Không hiện quảng cáo."
            );

            return;
        }

        // Đã đủ số lần thua và quảng cáo đã sẵn sàng.
        if (_interstitialAd != null &&
            _interstitialAd.CanShowAd())
        {
            Debug.Log(
                "INTERSTITIAL: Đủ số lần thua. " +
                "Bắt đầu hiển thị quảng cáo."
            );

            // Chọn lại chu kỳ ngẫu nhiên 3 hoặc 4
            // cho quảng cáo tiếp theo.
            PrepareNextInterstitialFrequency();

            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning(
                "INTERSTITIAL: Đã đủ số lần thua " +
                "nhưng quảng cáo chưa sẵn sàng. " +
                "Sẽ thử lại ở lần Game Over tiếp theo."
            );

            LoadInterstitial();
        }
    }

    private void DestroyInterstitial()
    {
        if (_interstitialAd == null)
        {
            return;
        }

        _interstitialAd.Destroy();
        _interstitialAd = null;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        // Không cleanup khi AdsManager trùng bị xóa
        // sau khi reload Scene.
        if (Instance != this)
        {
            return;
        }

        DestroyBanner();
        DestroyInterstitial();

        Instance = null;
    }
}