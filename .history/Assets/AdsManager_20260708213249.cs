using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Test Mode")]
    [Tooltip("Bật khi đang test. Tắt trước khi phát hành lên Google Play.")]
    [SerializeField]
    private bool _useTestAds = true;

    [Header("Android Ad Unit IDs")]
    [Tooltip("Interstitial Ad Unit ID thật.")]
    [SerializeField]
    private string _androidInterstitialAdUnitId;

    [Tooltip("Banner Ad Unit ID thật.")]
    [SerializeField]
    private string _androidBannerAdUnitId;

    [Header("Banner Settings")]
    [Tooltip("Số giây chờ trước khi tải lại Banner nếu tải thất bại.")]
    [SerializeField, Min(1f)]
    private float _bannerRetryDelay = 5f;

    // ID test chính thức của Google dành cho Android.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string TestBannerId =
        "ca-app-pub-3940256099942544/9214589741";

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

    private bool _sdkInitialized;
    private bool _sdkInitializing;

    private bool _bannerIsLoading;
    private bool _interstitialIsLoading;

    // Main Menu và Gameplay đều yêu cầu Banner hiển thị.
    private bool _bannerShouldBeVisible = true;

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
        // Scene reload nhưng chỉ giữ lại một AdsManager.
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

        // Đưa callback quảng cáo về Unity Main Thread.
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

            // Ưu tiên tải Banner trước.
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

        if (_bannerIsLoading)
        {
            Debug.Log(
                "BANNER: Đang có một yêu cầu tải."
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

        int deviceWidth =
            MobileAds.Utils.GetDeviceSafeWidth();

        AdSize bannerSize;

        if (deviceWidth > 0)
        {
            bannerSize =
                AdSize
                    .GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                        deviceWidth
                    );

            Debug.Log(
                "BANNER: Dùng Anchored Adaptive Banner. " +
                "Safe width = " +
                deviceWidth
            );
        }
        else
        {
            // Dự phòng nếu thiết bị chưa trả về chiều rộng hợp lệ.
            bannerSize = AdSize.Banner;

            Debug.LogWarning(
                "BANNER: Không lấy được safe width. " +
                "Tạm dùng Banner 320x50."
            );
        }

        _bannerView = new BannerView(
            CurrentBannerId,
            bannerSize,
            AdPosition.Bottom
        );

        RegisterBannerEvents(_bannerView);

        _bannerIsLoading = true;

        Debug.Log(
            "BANNER: Bắt đầu tải. ID = " +
            CurrentBannerId
        );

        _bannerView.LoadAd(
            new AdRequest()
        );
    }

    private void RegisterBannerEvents(
        BannerView banner
    )
    {
        banner.OnBannerAdLoaded += () =>
        {
            _bannerIsLoading = false;

            Debug.Log(
                "BANNER: Tải thành công."
            );

            if (_bannerView == null ||
                _bannerView.IsDestroyed)
            {
                Debug.LogWarning(
                    "BANNER: Banner đã bị hủy trước khi tải xong."
                );

                return;
            }

            Debug.Log(
                "BANNER: Kích thước thực tế = " +
                _bannerView.GetWidthInPixels() +
                " x " +
                _bannerView.GetHeightInPixels() +
                " px"
            );

            if (_bannerShouldBeVisible)
            {
                _bannerView.SetPosition(
                    AdPosition.Bottom
                );

                _bannerView.Show();

                Debug.Log(
                    "BANNER: Đã gọi Show() ở cạnh dưới."
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

        banner.OnBannerAdLoadFailed += error =>
        {
            _bannerIsLoading = false;

            Debug.LogError(
                "BANNER: Tải thất bại.\n" +
                "Message: " +
                error.GetMessage() +
                "\nChi tiết: " +
                error
            );

            // Quan trọng: xóa Banner lỗi để ShowBanner()
            // có thể tạo và tải lại Banner mới.
            DestroyBanner();

            if (_bannerShouldBeVisible)
            {
                ScheduleBannerRetry();
            }
        };

        banner.OnAdImpressionRecorded += () =>
        {
            Debug.Log(
                "BANNER: Đã ghi nhận lượt hiển thị."
            );
        };

        banner.OnAdClicked += () =>
        {
            Debug.Log(
                "BANNER: Đã ghi nhận lượt nhấn."
            );
        };
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
                "BANNER: Chờ SDK khởi tạo."
            );

            return;
        }

        if (_bannerView == null ||
            _bannerView.IsDestroyed)
        {
            Debug.Log(
                "BANNER: Chưa có Banner hợp lệ, tiến hành tải."
            );

            LoadBanner();
            return;
        }

        _bannerView.SetPosition(
            AdPosition.Bottom
        );

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
        LoadBanner();
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
        // Main Menu của game dùng Time.timeScale = 0,
        // nên bắt buộc dùng WaitForSecondsRealtime.
        yield return new WaitForSecondsRealtime(
            _bannerRetryDelay
        );

        _bannerRetryCoroutine = null;

        if (_bannerShouldBeVisible &&
            _sdkInitialized)
        {
            LoadBanner();
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
            Debug.LogWarning(
                "INTERSTITIAL: SDK chưa khởi tạo."
            );

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
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning(
                "INTERSTITIAL: Không thể mở.\n" +
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
        // AdsManager mới bị xóa do trùng Scene
        // không được phép hủy quảng cáo của Instance cũ.
        if (Instance != this)
        {
            return;
        }

        StopBannerRetry();
        DestroyBanner();
        DestroyInterstitial();

        Instance = null;
    }
}