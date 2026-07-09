using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Test Mode")]
    [Tooltip("Bật khi đang test. Chỉ tắt trước khi phát hành lên Google Play.")]
    [SerializeField]
    private bool _useTestAds = true;

    [Header("Android Ad Unit IDs")]
    [Tooltip("Interstitial Ad Unit ID thật.")]
    [SerializeField]
    private string _androidInterstitialAdUnitId;

    [Tooltip("Banner Ad Unit ID thật.")]
    [SerializeField]
    private string _androidBannerAdUnitId;

    [Header("Banner Position")]
    [Tooltip("Khoảng nâng Banner lên khỏi đáy vùng an toàn, tính bằng pixel.")]
    [SerializeField, Min(0)]
    private int _bannerBottomOffsetPixels = 60;

    [Tooltip("Thời gian chờ tải lại Banner nếu tải thất bại.")]
    [SerializeField, Min(1f)]
    private float _bannerRetryDelay = 5f;

    // ID test chính thức của Google cho Android.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string TestBannerId =
        "ca-app-pub-3940256099942544/9214589741";

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

    private bool _sdkInitialized;
    private bool _sdkInitializing;

    private bool _bannerIsLoading;
    private bool _bannerLoaded;
    private bool _bannerShouldBeVisible = true;

    private bool _interstitialIsLoading;

    private Coroutine _bannerRetryCoroutine;

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
        // Khi Scene reload, chỉ giữ một AdsManager duy nhất.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (_sdkInitialized || _sdkInitializing)
        {
            return;
        }

        _sdkInitializing = true;

        Debug.Log("ADMOB: Bắt đầu khởi tạo SDK.");

        // Các callback quảng cáo sẽ chạy trên Unity Main Thread.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize(initializationStatus =>
        {
            _sdkInitializing = false;

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

            CreateAndLoadBanner();
            LoadInterstitial();
        });
    }

    // =========================================================
    // BANNER
    // =========================================================

    private void CreateAndLoadBanner()
    {
        if (!_sdkInitialized)
        {
            Debug.LogWarning(
                "BANNER: SDK chưa khởi tạo."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentBannerId))
        {
            Debug.LogError(
                "BANNER: Banner Ad Unit ID đang bị trống."
            );

            return;
        }

        StopBannerRetry();
        DestroyBanner();

        // Google trả về chiều rộng an toàn theo dp.
        int safeDeviceWidth =
            MobileAds.Utils.GetDeviceSafeWidth();

        AdSize adaptiveSize;

        if (safeDeviceWidth > 0)
        {
            adaptiveSize =
                AdSize
                    .GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                        safeDeviceWidth
                    );

            Debug.Log(
                "BANNER: Safe width = " +
                safeDeviceWidth
            );
        }
        else
        {
            // Dự phòng khi không lấy được chiều rộng.
            adaptiveSize = AdSize.Banner;

            Debug.LogWarning(
                "BANNER: Không lấy được safe width. " +
                "Tạm sử dụng Banner 320x50."
            );
        }

        // Tạo ở Bottom trước, sau khi tải xong sẽ đặt lại
        // bằng tọa độ để nâng Banner lên.
        _bannerView = new BannerView(
            CurrentBannerId,
            adaptiveSize,
            AdPosition.Bottom
        );

        RegisterBannerEvents(_bannerView);
        RequestBannerAd();
    }

    private void RegisterBannerEvents(
        BannerView bannerView
    )
    {
        bannerView.OnBannerAdLoaded += () =>
        {
            _bannerIsLoading = false;
            _bannerLoaded = true;

            StopBannerRetry();

            Debug.Log(
                "BANNER: Tải thành công."
            );

            if (_bannerView == null ||
                _bannerView.IsDestroyed)
            {
                return;
            }

            Debug.Log(
                "BANNER: Kích thước thực tế = " +
                _bannerView.GetWidthInPixels() +
                " x " +
                _bannerView.GetHeightInPixels() +
                " px"
            );

            PositionBannerAboveBottomSafeArea();

            if (_bannerShouldBeVisible)
            {
                _bannerView.Show();

                Debug.Log(
                    "BANNER: Đã hiển thị."
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

        bannerView.OnBannerAdLoadFailed += error =>
        {
            _bannerIsLoading = false;
            _bannerLoaded = false;

            Debug.LogError(
                "BANNER: Tải thất bại.\n" +
                "Message: " +
                error.GetMessage() +
                "\nChi tiết: " +
                error
            );

            if (_bannerShouldBeVisible)
            {
                ScheduleBannerRetry();
            }
        };

        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log(
                "BANNER: Đã ghi nhận lượt hiển thị."
            );
        };

        bannerView.OnAdClicked += () =>
        {
            Debug.Log(
                "BANNER: Đã ghi nhận lượt nhấn."
            );
        };
    }

    private void RequestBannerAd()
    {
        if (_bannerView == null ||
            _bannerView.IsDestroyed)
        {
            CreateAndLoadBanner();
            return;
        }

        if (_bannerIsLoading)
        {
            return;
        }

        _bannerIsLoading = true;
        _bannerLoaded = false;

        Debug.Log(
            "BANNER: Bắt đầu gửi yêu cầu tải. ID = " +
            CurrentBannerId
        );

        _bannerView.LoadAd(
            new AdRequest()
        );
    }

    /// <summary>
    /// Đặt Banner cao hơn đáy màn hình và thanh điều hướng.
    /// Tọa độ Banner lấy góc trên bên trái làm gốc.
    /// </summary>
    private void PositionBannerAboveBottomSafeArea()
    {
        if (_bannerView == null ||
            _bannerView.IsDestroyed ||
            !_bannerLoaded)
        {
            return;
        }

        int bannerWidth = Mathf.RoundToInt(
            _bannerView.GetWidthInPixels()
        );

        int bannerHeight = Mathf.RoundToInt(
            _bannerView.GetHeightInPixels()
        );

        if (bannerWidth <= 0 || bannerHeight <= 0)
        {
            Debug.LogWarning(
                "BANNER: Kích thước Banner chưa hợp lệ."
            );

            _bannerView.SetPosition(
                AdPosition.Bottom
            );

            return;
        }

        Rect safeArea = Screen.safeArea;

        // Căn giữa Banner trong vùng an toàn.
        int positionX = Mathf.RoundToInt(
            safeArea.xMin +
            (safeArea.width - bannerWidth) / 2f
        );

        /*
         * Screen.safeArea dùng gốc ở góc dưới bên trái.
         * BannerView dùng gốc ở góc trên bên trái.
         *
         * Vì vậy phải chuyển đổi tọa độ Y.
         */
        int positionY = Mathf.RoundToInt(
            Screen.height -
            safeArea.yMin -
            bannerHeight -
            _bannerBottomOffsetPixels
        );

        int maximumX = Mathf.Max(
            0,
            Screen.width - bannerWidth
        );

        int maximumY = Mathf.Max(
            0,
            Screen.height - bannerHeight
        );

        positionX = Mathf.Clamp(
            positionX,
            0,
            maximumX
        );

        positionY = Mathf.Clamp(
            positionY,
            0,
            maximumY
        );

        _bannerView.SetPosition(
            positionX,
            positionY
        );

        Debug.Log(
            "BANNER POSITION:" +
            "\nScreen = " +
            Screen.width +
            " x " +
            Screen.height +
            "\nSafe Area = " +
            safeArea +
            "\nBanner = " +
            bannerWidth +
            " x " +
            bannerHeight +
            "\nPosition = " +
            positionX +
            ", " +
            positionY +
            "\nBottom Offset = " +
            _bannerBottomOffsetPixels
        );
    }

    public void ShowBanner()
    {
        _bannerShouldBeVisible = true;

        Debug.Log(
            "BANNER: GameManager yêu cầu hiển thị."
        );

        if (!_sdkInitialized)
        {
            Debug.Log(
                "BANNER: Đang chờ SDK khởi tạo."
            );

            return;
        }

        if (_bannerView == null ||
            _bannerView.IsDestroyed)
        {
            CreateAndLoadBanner();
            return;
        }

        if (!_bannerLoaded)
        {
            if (!_bannerIsLoading)
            {
                RequestBannerAd();
            }

            return;
        }

        PositionBannerAboveBottomSafeArea();
        _bannerView.Show();

        Debug.Log(
            "BANNER: Đã gọi Show()."
        );
    }

    public void HideBanner()
    {
        _bannerShouldBeVisible = false;

        StopBannerRetry();

        Debug.Log(
            "BANNER: GameManager yêu cầu ẩn."
        );

        if (_bannerView != null &&
            !_bannerView.IsDestroyed)
        {
            _bannerView.Hide();
        }
    }

    public void ReloadBanner()
    {
        _bannerShouldBeVisible = true;

        CreateAndLoadBanner();
    }

    private void ScheduleBannerRetry()
    {
        if (_bannerRetryCoroutine != null)
        {
            return;
        }

        Debug.Log(
            "BANNER: Sẽ tải lại sau " +
            _bannerRetryDelay +
            " giây."
        );

        _bannerRetryCoroutine =
            StartCoroutine(
                RetryBannerAfterDelay()
            );
    }

    private IEnumerator RetryBannerAfterDelay()
    {
        // Main Menu đang Time.timeScale = 0,
        // nên phải dùng thời gian thực.
        yield return new WaitForSecondsRealtime(
            _bannerRetryDelay
        );

        _bannerRetryCoroutine = null;

        if (_bannerShouldBeVisible &&
            _sdkInitialized)
        {
            RequestBannerAd();
        }
    }

    private void StopBannerRetry()
    {
        if (_bannerRetryCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            _bannerRetryCoroutine
        );

        _bannerRetryCoroutine = null;
    }

    private void DestroyBanner()
    {
        StopBannerRetry();

        _bannerIsLoading = false;
        _bannerLoaded = false;

        if (_bannerView == null)
        {
            return;
        }

        _bannerView.Destroy();
        _bannerView = null;
    }

    // =========================================================
    // INTERSTITIAL GAME OVER
    // =========================================================

    private void LoadInterstitial()
    {
        if (!_sdkInitialized)
        {
            return;
        }

        if (_interstitialIsLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                CurrentInterstitialId))
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
                    Debug.LogWarning(
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

            // Interstitial chỉ được sử dụng một lần,
            // nên phải tải quảng cáo mới.
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning(
                "INTERSTITIAL: Không thể mở quảng cáo.\n" +
                error.GetMessage()
            );

            DestroyInterstitial();
            LoadInterstitial();
        };
    }

    public void ShowGameOverAd()
    {
        if (_interstitialAd != null &&
            _interstitialAd.CanShowAd())
        {
            Debug.Log(
                "INTERSTITIAL: Hiển thị quảng cáo Game Over."
            );

            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning(
                "INTERSTITIAL: Quảng cáo chưa sẵn sàng. " +
                "Bỏ qua lần Game Over này."
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
        // Không cleanup nếu đây chỉ là AdsManager trùng
        // bị xóa sau khi Scene reload.
        if (Instance != this)
        {
            return;
        }

        DestroyBanner();
        DestroyInterstitial();

        Instance = null;
    }
}