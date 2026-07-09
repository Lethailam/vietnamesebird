using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Test Mode")]
    [Tooltip("Bật khi đang test. Tắt trước khi phát hành lên Google Play.")]
    [SerializeField] private bool _useTestAds = true;

    [Header("Android Ad Unit IDs")]
    [Tooltip("Interstitial Ad Unit ID thật.")]
    [SerializeField] private string _androidInterstitialAdUnitId;

    [Tooltip("Banner Ad Unit ID thật.")]
    [SerializeField] private string _androidBannerAdUnitId;

    // ID Interstitial test chính thức của Google.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    // ID Anchored Adaptive Banner test chính thức của Google.
    private const string TestBannerId =
        "ca-app-pub-3940256099942544/9214589741";

    private InterstitialAd _interstitialAd;
    private BannerView _bannerView;

    // Khi Banner tải xong, biến này quyết định hiện hay ẩn.
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
        // Chỉ giữ một AdsManager khi Scene reload.
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
        MobileAds.Initialize(initializationStatus =>
        {
            if (initializationStatus == null)
            {
                Debug.LogError("AdMob khởi tạo thất bại.");
                return;
            }

            Debug.Log("AdMob khởi tạo thành công.");

            LoadInterstitial();
            LoadBanner();
        });
    }

    // =========================================================
    // BANNER
    // =========================================================

    private void LoadBanner()
    {
        DestroyBanner();

        // Lấy chiều rộng an toàn của thiết bị.
        int deviceWidth = MobileAds.Utils.GetDeviceSafeWidth();

        // Tạo kích thước Banner thích ứng với thiết bị.
        AdSize adaptiveSize =
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                deviceWidth
            );

        // Neo Banner ở cạnh dưới màn hình.
        _bannerView = new BannerView(
            CurrentBannerId,
            adaptiveSize,
            AdPosition.Bottom
        );

        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner đã tải thành công.");

            if (_bannerShouldBeVisible)
            {
                _bannerView.Show();
            }
            else
            {
                _bannerView.Hide();
            }
        };

        _bannerView.OnBannerAdLoadFailed += error =>
        {
            Debug.LogWarning(
                "Banner tải thất bại: " +
                error.GetMessage()
            );
        };

        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner đã ghi nhận một lượt hiển thị.");
        };

        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner đã được bấm.");
        };

        AdRequest request = new AdRequest();
        _bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        _bannerShouldBeVisible = true;

        if (_bannerView != null)
        {
            _bannerView.Show();
        }
    }

    public void HideBanner()
    {
        _bannerShouldBeVisible = false;

        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
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
        DestroyInterstitial();

        Debug.Log("Đang tải quảng cáo Game Over...");

        AdRequest request = new AdRequest();

        InterstitialAd.Load(
            CurrentInterstitialId,
            request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Tải Interstitial thất bại: " +
                        error
                    );

                    return;
                }

                _interstitialAd = ad;

                Debug.Log(
                    "Quảng cáo Game Over đã tải thành công."
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
                "Quảng cáo Game Over đã mở."
            );
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log(
                "Người chơi đã đóng quảng cáo Game Over."
            );

            DestroyInterstitial();

            // Tải quảng cáo mới cho lần Game Over tiếp theo.
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning(
                "Không thể mở Interstitial: " +
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
                "Hiển thị quảng cáo Game Over."
            );

            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning(
                "Interstitial chưa sẵn sàng. " +
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

    private void OnDestroy()
    {
        // Không cleanup khi AdsManager trùng bị xóa.
        if (Instance != this)
        {
            return;
        }

        DestroyBanner();
        DestroyInterstitial();

        Instance = null;
    }
}