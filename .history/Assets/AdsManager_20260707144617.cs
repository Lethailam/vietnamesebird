using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("AdMob")]
    [Tooltip("Luôn bật khi đang test. Chỉ tắt khi chuẩn bị phát hành.")]
    [SerializeField] private bool _useTestAd = true;

    [Tooltip("Dán Interstitial Ad Unit ID thật của bạn vào đây.")]
    [SerializeField] private string _androidInterstitialAdUnitId;

    // ID quảng cáo Interstitial thử nghiệm chính thức của Google.
    private const string AndroidTestInterstitialId =
        "ca-app-pub-3940256099942544/1033173712";

    private InterstitialAd _interstitialAd;

    private string CurrentAdUnitId
    {
        get
        {
            if (_useTestAd)
            {
                return AndroidTestInterstitialId;
            }

            return _androidInterstitialAdUnitId;
        }
    }

    private void Awake()
    {
        // Giữ lại duy nhất một AdsManager khi Scene được tải lại.
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
        });
    }

    private void LoadInterstitial()
    {
        DestroyInterstitial();

        Debug.Log("Đang tải quảng cáo Game Over...");

        AdRequest request = new AdRequest();

        InterstitialAd.Load(
            CurrentAdUnitId,
            request,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning(
                        "Tải quảng cáo thất bại: " + error
                    );

                    return;
                }

                _interstitialAd = ad;

                Debug.Log("Quảng cáo Game Over đã tải xong.");

                RegisterAdEvents(ad);
            }
        );
    }

    private void RegisterAdEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Quảng cáo Game Over đã mở.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Người chơi đã đóng quảng cáo.");

            DestroyInterstitial();

            // Tải trước quảng cáo mới cho lần Game Over tiếp theo.
            LoadInterstitial();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            Debug.LogWarning(
                "Không thể mở quảng cáo: " +
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
            Debug.Log("Hiển thị quảng cáo Game Over.");

            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning(
                "Quảng cáo chưa tải xong. Bỏ qua lần Game Over này."
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
        // Không cleanup khi AdsManager trùng bị xóa lúc reload Scene.
        if (Instance != this)
        {
            return;
        }

        DestroyInterstitial();
        Instance = null;
    }
}