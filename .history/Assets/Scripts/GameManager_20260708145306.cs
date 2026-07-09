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

    // ID test chính thức của Google dành cho Android.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string TestBannerId =
        "ca-app-pub-3940256099942544/6300978111";

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

    private bool _sdkInitialized;
    private bool _bannerShouldBeVisible = true;

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

    private void InitializeAds()
    {
        if (_sdkInitialized)
        {
            return;
        }

        Debug.Log("ADMOB: Bắt đầu khởi tạo SDK.");

        // Đưa callback quảng cáo về Unity Main Thread.
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
            "BANNER: Bắt đầu tải. ID đang dùng: " +
            CurrentBannerId
        );

        // Dùng Banner cố định 320x50 để kiểm tra ổn định trước.
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

            Debug.Log(
                "BANNER: Kích thước thật = " +
                _bannerView.GetWidthInPixels() +
                " x " +
                _bannerView.GetHeightInPixels()
            );

            if (_bannerShouldBeVisible)
            {
                Debug.Log(
                    "BANNER: Hiển thị ở cạnh dưới."
                );

                _bannerView.Show();
            }
            else
            {
                Debug.Log(
                    "BANNER: Đã tải nhưng đang được yêu cầu ẩn."
                );

                _bannerView.Hide();
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
                "BANNER: SDK chưa khởi tạo, sẽ hiện sau khi tải xong."
            );

            return;
        }

        if (_bannerView == null)
        {
            Debug.LogWarning(
                "BANNER: Chưa có BannerView, tiến hành tải lại."
            );

            LoadBanner();
            return;
        }

        _bannerView.SetPosition(
            AdPosition.Bottom
        );

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

        Debug.Log(
            "BANNER: Hủy BannerView cũ."
        );

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

        DestroyInterstitial();

        if (string.IsNullOrWhiteSpace(
                CurrentInterstitialId))
        {
            Debug.LogError(
                "INTERSTITIAL: Ad Unit ID đang bị trống."
            );

            return;
        }

        Debug.Log(
            "INTERSTITIAL: Bắt đầu tải."
        );

        InterstitialAd.Load(
            CurrentInterstitialId,
            new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
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
        InterstitialAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log(
                "INTERSTITIAL: Đã mở."
            );
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log(
                "INTERSTITIAL: Đã đóng."
            );

            DestroyInterstitial();
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogError(
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
                "INTERSTITIAL: Hiển thị Game Over Ads."
            );

            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning(
                "INTERSTITIAL: Chưa sẵn sàng, bỏ qua lần này."
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

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        DestroyBanner();
        DestroyInterstitial();

        Instance = null;
    }
}