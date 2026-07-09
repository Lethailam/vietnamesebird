using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Banner Settings")]
    [Tooltip("Thời gian chờ trước khi thử tải lại Banner.")]
    [SerializeField, Min(1f)]
    private float _bannerRetryDelay = 5f;

    // ID test chính thức của Google dành cho Android.
    private const string TestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private const string TestBannerId =
        "ca-app-pub-3940256099942544/9214589741";

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;

    private bool _sdkInitialized;
    private bool _sdkInitializing;

    private bool _bannerIsLoading;
    private bool _bannerLoaded;
    private bool _bannerShouldBeVisible = true;

    private bool _interstitialIsLoading;
    private bool _sceneReloadPending;

    [Header("Interstitial Frequency")]
    [SerializeField, Min(1)]
    private int _gameOversBeforeInterstitial = 3;

private int _gameOverCountSinceLastInterstitial;

    private Coroutine _bannerRetryCoroutine;

    // Dùng để vô hiệu hóa callback của yêu cầu tải cũ.
    private int _bannerGeneration;
    private int _interstitialGeneration;

    private string CurrentBannerId
    {
        get
        {
            return _useTestAds
                ? TestBannerId
                : _androidBannerAdUnitId;
        }
    }

    private string CurrentInterstitialId
    {
        get
        {
            return _useTestAds
                ? TestInterstitialId
                : _androidInterstitialAdUnitId;
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

        // Giúp callback quảng cáo chạy trên Unity Main Thread.
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

            LoadBanner();
            LoadInterstitial();
        });
    }

    // =========================================================
    // XỬ LÝ RELOAD SCENE
    // =========================================================

    /// <summary>
    /// Phải gọi trước khi Scene được reload.
    /// Việc này tránh Banner của Scene cũ trở thành
    /// MissingReferenceException trong Unity Editor.
    /// </summary>
    public void PrepareForSceneReload()
    {
        Debug.Log(
            "ADMOB: Chuẩn bị reload Scene."
        );

        _sceneReloadPending = true;
        _bannerShouldBeVisible = false;

        StopBannerRetry();
        DestroyBannerSafely();
        DestroyInterstitialSafely();
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode
    )
    {
        if (!_sceneReloadPending)
        {
            return;
        }

        _sceneReloadPending = false;

        StartCoroutine(
            RecreateAdsAfterSceneLoaded()
        );
    }

    private IEnumerator RecreateAdsAfterSceneLoaded()
    {
        // Chờ GameObject của Scene mới được khởi tạo.
        yield return null;

        if (!_sdkInitialized)
        {
            yield break;
        }

        Debug.Log(
            "ADMOB: Scene mới đã tải. Tạo lại quảng cáo."
        );

        /*
         * GameManager.Start() của Scene mới sẽ quyết định
         * Banner phải hiện hay ẩn bằng ShowBanner()/HideBanner().
         */
        LoadBanner();
        LoadInterstitial();
    }

    // =========================================================
    // BANNER
    // =========================================================

    private void LoadBanner()
    {
        if (!_sdkInitialized)
        {
            Debug.Log(
                "BANNER: Đang chờ SDK khởi tạo."
            );

            return;
        }

        if (_sceneReloadPending)
        {
            Debug.Log(
                "BANNER: Đang reload Scene, chưa tải Banner."
            );

            return;
        }

        if (_bannerIsLoading)
        {
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
        DestroyBannerSafely();

        int safeWidth =
            MobileAds.Utils.GetDeviceSafeWidth();

        AdSize bannerSize;

        if (safeWidth > 0)
        {
            bannerSize =
                AdSize
                    .GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
                        safeWidth
                    );

            Debug.Log(
                "BANNER: Dùng Adaptive Banner. Width = " +
                safeWidth
            );
        }
        else
        {
            bannerSize = AdSize.Banner;

            Debug.LogWarning(
                "BANNER: Không lấy được Safe Width. " +
                "Sử dụng Banner 320x50."
            );
        }

        int currentGeneration = _bannerGeneration;

        BannerView newBanner;

        try
        {
            /*
             * Không dùng tọa độ tùy chỉnh.
             * AdPosition.Bottom sẽ để SDK tự đặt Banner
             * đúng tại cạnh dưới màn hình.
             */
            newBanner = new BannerView(
                CurrentBannerId,
                bannerSize,
                AdPosition.Bottom
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "BANNER: Không thể tạo BannerView.\n" +
                exception
            );

            ScheduleBannerRetry();
            return;
        }

        _bannerView = newBanner;
        _bannerLoaded = false;
        _bannerIsLoading = true;

        RegisterBannerEvents(
            newBanner,
            currentGeneration
        );

        Debug.Log(
            "BANNER: Bắt đầu tải. ID = " +
            CurrentBannerId
        );

        try
        {
            newBanner.LoadAd(
                new AdRequest()
            );
        }
        catch (Exception exception)
        {
            _bannerIsLoading = false;

            Debug.LogError(
                "BANNER: Lỗi khi gửi yêu cầu tải.\n" +
                exception
            );

            RecoverBannerReference();
        }
    }

    private void RegisterBannerEvents(
        BannerView banner,
        int generation
    )
    {
        banner.OnBannerAdLoaded += () =>
        {
            // Bỏ qua callback của Banner cũ.
            if (generation != _bannerGeneration ||
                !ReferenceEquals(_bannerView, banner))
            {
                return;
            }

            _bannerIsLoading = false;
            _bannerLoaded = true;

            StopBannerRetry();

            Debug.Log(
                "BANNER: Tải thành công."
            );

            if (_bannerShouldBeVisible)
            {
                ShowCurrentBannerSafely();
            }
            else
            {
                HideCurrentBannerSafely();
            }
        };

        banner.OnBannerAdLoadFailed += error =>
        {
            if (generation != _bannerGeneration ||
                !ReferenceEquals(_bannerView, banner))
            {
                return;
            }

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

        banner.OnAdImpressionRecorded += () =>
        {
            if (generation != _bannerGeneration)
            {
                return;
            }

            Debug.Log(
                "BANNER: Đã ghi nhận lượt hiển thị."
            );
        };

        banner.OnAdClicked += () =>
        {
            if (generation != _bannerGeneration)
            {
                return;
            }

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
                "BANNER: SDK chưa khởi tạo. " +
                "Banner sẽ hiện sau khi SDK sẵn sàng."
            );

            return;
        }

        if (_sceneReloadPending)
        {
            Debug.Log(
                "BANNER: Đang chờ Scene mới tải xong."
            );

            return;
        }

        if (_bannerView == null)
        {
            LoadBanner();
            return;
        }

        if (!_bannerLoaded)
        {
            if (!_bannerIsLoading)
            {
                RequestBannerAgain();
            }

            return;
        }

        ShowCurrentBannerSafely();
    }

    public void HideBanner()
    {
        _bannerShouldBeVisible = false;

        StopBannerRetry();

        Debug.Log(
            "BANNER: GameManager yêu cầu ẩn."
        );

        HideCurrentBannerSafely();
    }

    private void ShowCurrentBannerSafely()
    {
        if (_bannerView == null)
        {
            return;
        }

        try
        {
            _bannerView.SetPosition(
                AdPosition.Bottom
            );

            _bannerView.Show();

            Debug.Log(
                "BANNER: Đã gọi Show() tại Bottom."
            );
        }
        catch (MissingReferenceException exception)
        {
            Debug.LogWarning(
                "BANNER: Banner của Scene cũ đã bị hủy.\n" +
                exception.Message
            );

            RecoverBannerReference();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "BANNER: Không thể hiển thị Banner.\n" +
                exception
            );

            RecoverBannerReference();
        }
    }

    private void HideCurrentBannerSafely()
    {
        if (_bannerView == null)
        {
            return;
        }

        try
        {
            _bannerView.Hide();
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning(
                "BANNER: Banner cần ẩn đã bị Scene cũ hủy."
            );

            DestroyBannerSafely();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "BANNER: Không thể ẩn Banner.\n" +
                exception
            );

            DestroyBannerSafely();
        }
    }

    private void RequestBannerAgain()
    {
        if (_bannerView == null)
        {
            LoadBanner();
            return;
        }

        if (_bannerIsLoading)
        {
            return;
        }

        _bannerIsLoading = true;
        _bannerLoaded = false;

        Debug.Log(
            "BANNER: Thử tải lại quảng cáo."
        );

        try
        {
            _bannerView.LoadAd(
                new AdRequest()
            );
        }
        catch (Exception exception)
        {
            _bannerIsLoading = false;

            Debug.LogWarning(
                "BANNER: Banner cũ không còn hợp lệ.\n" +
                exception
            );

            RecoverBannerReference();
        }
    }

    private void RecoverBannerReference()
    {
        DestroyBannerSafely();

        if (_bannerShouldBeVisible &&
            _sdkInitialized &&
            !_sceneReloadPending)
        {
            ScheduleBannerRetry();
        }
    }

    private void ScheduleBannerRetry()
    {
        if (_bannerRetryCoroutine != null ||
            !_bannerShouldBeVisible ||
            _sceneReloadPending)
        {
            return;
        }

        Debug.Log(
            "BANNER: Sẽ thử tải lại sau " +
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
        /*
         * Main Menu có Time.timeScale = 0,
         * vì vậy phải dùng WaitForSecondsRealtime.
         */
        yield return new WaitForSecondsRealtime(
            _bannerRetryDelay
        );

        _bannerRetryCoroutine = null;

        if (_bannerShouldBeVisible &&
            _sdkInitialized &&
            !_sceneReloadPending)
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

    private void DestroyBannerSafely()
    {
        StopBannerRetry();

        // Vô hiệu hóa tất cả callback của Banner cũ.
        _bannerGeneration++;

        _bannerIsLoading = false;
        _bannerLoaded = false;

        BannerView bannerToDestroy = _bannerView;
        _bannerView = null;

        if (bannerToDestroy == null)
        {
            return;
        }

        try
        {
            bannerToDestroy.Destroy();
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning(
                "BANNER: GameObject Banner đã bị Scene hủy trước đó."
            );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "BANNER: Lỗi khi hủy Banner.\n" +
                exception
            );
        }
    }

    // =========================================================
    // INTERSTITIAL GAME OVER
    // =========================================================

    private void LoadInterstitial()
    {
        if (!_sdkInitialized ||
            _sceneReloadPending ||
            _interstitialIsLoading)
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

        DestroyInterstitialSafely();

        int currentGeneration =
            _interstitialGeneration;

        _interstitialIsLoading = true;

        Debug.Log(
            "INTERSTITIAL: Bắt đầu tải."
        );

        InterstitialAd.Load(
            CurrentInterstitialId,
            new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                /*
                 * Nếu Scene đã reload trong khi quảng cáo tải,
                 * bỏ quảng cáo cũ này.
                 */
                if (currentGeneration !=
                    _interstitialGeneration)
                {
                    if (ad != null)
                    {
                        ad.Destroy();
                    }

                    return;
                }

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
            if (!ReferenceEquals(
                    _interstitialAd,
                    ad))
            {
                return;
            }

            Debug.Log(
                "INTERSTITIAL: Người chơi đã đóng quảng cáo."
            );

            DestroyInterstitialSafely();
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            if (!ReferenceEquals(
                    _interstitialAd,
                    ad))
            {
                return;
            }

            Debug.LogWarning(
                "INTERSTITIAL: Không thể mở.\n" +
                error.GetMessage()
            );

            DestroyInterstitialSafely();
            LoadInterstitial();
        };
    }

    public void ShowGameOverAd()
    {
        _gameOverCountSinceLastInterstitial++;

        Debug.Log(
            "INTERSTITIAL: Số lần thua hiện tại = " +
            _gameOverCountSinceLastInterstitial +
            "/" +
            _gameOversBeforeInterstitial
        );

        // Chưa đủ 3 lần thua thì không hiện quảng cáo.
        if (_gameOverCountSinceLastInterstitial <
            _gameOversBeforeInterstitial)
        {
            Debug.Log(
                "INTERSTITIAL: Chưa đủ số lần thua, không hiện quảng cáo."
            );

            // Vẫn đảm bảo quảng cáo được tải sẵn cho lần sau.
            if (_interstitialAd == null ||
                !_interstitialAd.CanShowAd())
            {
                LoadInterstitial();
            }

            return;
        }

        // Đủ 3 lần thua thì mới hiện quảng cáo.
        if (_interstitialAd != null &&
            _interstitialAd.CanShowAd())
        {
            Debug.Log(
                "INTERSTITIAL: Đủ số lần thua. Hiển thị quảng cáo Game Over."
            );

            // Reset bộ đếm vì lần này đã hiện quảng cáo.
            _gameOverCountSinceLastInterstitial = 0;

            try
            {
                _interstitialAd.Show();
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "INTERSTITIAL: Lỗi khi hiển thị.\n" +
                    exception
                );

                DestroyInterstitialSafely();
                LoadInterstitial();
            }
        }
        else
        {
            Debug.LogWarning(
                "INTERSTITIAL: Đã đủ số lần thua nhưng quảng cáo chưa sẵn sàng. " +
                "Sẽ thử lại ở lần Game Over tiếp theo."
            );

            // Không reset counter ở đây.
            // Vì chưa hiện được quảng cáo thì lần thua sau sẽ thử hiện tiếp.
            LoadInterstitial();
        }
    }

    private void DestroyInterstitialSafely()
    {
        // Vô hiệu hóa callback tải cũ.
        _interstitialGeneration++;
        _interstitialIsLoading = false;

        InterstitialAd adToDestroy =
            _interstitialAd;

        _interstitialAd = null;

        if (adToDestroy == null)
        {
            return;
        }

        try
        {
            adToDestroy.Destroy();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "INTERSTITIAL: Lỗi khi hủy quảng cáo.\n" +
                exception
            );
        }
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        /*
         * AdsManager duplicate của Scene mới bị xóa
         * không được phép dọn quảng cáo của Instance cũ.
         */
        if (Instance != this)
        {
            return;
        }

        StopBannerRetry();
        DestroyBannerSafely();
        DestroyInterstitialSafely();

        Instance = null;
    }
}