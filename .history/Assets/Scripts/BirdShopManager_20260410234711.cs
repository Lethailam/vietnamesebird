// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class BirdShopManager : MonoBehaviour
// {
//     public static BirdShopManager Instance;

//     public enum ShopCategory
//     {
//         Bird,
//         Background
//     }

//     [System.Serializable]
//     public class BackgroundShopItem
//     {
//         public string id;
//         public Sprite previewSprite;     // ảnh preview trong shop
//         public Sprite backgroundSprite;  // background sẽ equip
//         public int price;
//     }

//     [Header("Bird Shop Data")]
//     [SerializeField] private Sprite[] _birdSprites;
//     [SerializeField] private int[] _birdPrices;

//     [Header("Background Shop Data")]
//     [SerializeField] private BackgroundShopItem[] _backgroundItems;

//     [Header("UI")]
//     [SerializeField] private GameObject _shopPanel;
//     [SerializeField] private Image _previewImage;
//     [SerializeField] private TextMeshProUGUI _itemNameText;
//     [SerializeField] private TextMeshProUGUI _priceText;
//     [SerializeField] private TextMeshProUGUI _statusText;
//     [SerializeField] private Button _buyButton;
//     [SerializeField] private Button _equipButton;
//     [SerializeField] private TextMeshProUGUI _buyButtonText;
//     [SerializeField] private TextMeshProUGUI _equipButtonText;
//     [SerializeField] private TextMeshProUGUI _categoryText;

//     [Header("Scene References")]
//     [SerializeField] private SpriteRenderer _playerSpriteRenderer;
//     [SerializeField] private SpriteRenderer _backgroundRenderer;

//     private ShopCategory _currentCategory = ShopCategory.Bird;
//     private int _currentBirdIndex = 0;
//     private int _currentBackgroundIndex = 0;

//     private const string SELECTED_BIRD_KEY = "SelectedBird";
//     private const string SELECTED_BG_KEY = "SelectedShopBackground";

//     private void Awake()
//     {
//         Instance = this;
//     }

//     private void Start()
//     {
//         LoadSelectedBird();
//         LoadSelectedBackground();
//         _shopPanel.SetActive(false);
//     }

//     // =========================
//     // OPEN/CLOSE SHOP
//     // =========================
//     public void OpenShopAfterLevelUp()
//     {
//         _shopPanel.SetActive(true);
//         Time.timeScale = 0f;

//         // Mặc định mở tab Bird trước
//         _currentCategory = ShopCategory.Bird;
//         RefreshUI();
//     }

//     public void CloseShopAndContinue()
//     {
//         _shopPanel.SetActive(false);

//         if (LevelManager.Instance != null)
//         {
//             LevelManager.Instance.ContinueToNextLevel();
//         }
//     }

//     // =========================
//     // CATEGORY
//     // =========================
//     public void ShowBirdCategory()
//     {
//         _currentCategory = ShopCategory.Bird;
//         RefreshUI();
//     }

//     public void ShowBackgroundCategory()
//     {
//         _currentCategory = ShopCategory.Background;
//         RefreshUI();
//     }

//     // =========================
//     // NAVIGATION
//     // =========================
//     public void NextItem()
//     {
//         if (_currentCategory == ShopCategory.Bird)
//         {
//             _currentBirdIndex++;
//             if (_currentBirdIndex >= _birdSprites.Length)
//                 _currentBirdIndex = 0;
//         }
//         else
//         {
//             _currentBackgroundIndex++;
//             if (_currentBackgroundIndex >= _backgroundItems.Length)
//                 _currentBackgroundIndex = 0;
//         }

//         RefreshUI();
//     }

//     public void PreviousItem()
//     {
//         if (_currentCategory == ShopCategory.Bird)
//         {
//             _currentBirdIndex--;
//             if (_currentBirdIndex < 0)
//                 _currentBirdIndex = _birdSprites.Length - 1;
//         }
//         else
//         {
//             _currentBackgroundIndex--;
//             if (_currentBackgroundIndex < 0)
//                 _currentBackgroundIndex = _backgroundItems.Length - 1;
//         }

//         RefreshUI();
//     }

//     // =========================
//     // BUY / EQUIP
//     // =========================
//     public void BuyCurrentItem()
//     {
//         if (CoinWallet.Instance == null) return;

//         if (_currentCategory == ShopCategory.Bird)
//         {
//             int price = _birdPrices[_currentBirdIndex];

//             if (IsBirdUnlocked(_currentBirdIndex)) return;

//             if (CoinWallet.Instance.TrySpendCoins(price))
//             {
//                 PlayerPrefs.SetInt(GetBirdUnlockKey(_currentBirdIndex), 1);
//                 PlayerPrefs.Save();
//             }
//         }
//         else
//         {
//             int price = _backgroundItems[_currentBackgroundIndex].price;

//             if (IsBackgroundUnlocked(_currentBackgroundIndex)) return;

//             if (CoinWallet.Instance.TrySpendCoins(price))
//             {
//                 PlayerPrefs.SetInt(GetBackgroundUnlockKey(_currentBackgroundIndex), 1);
//                 PlayerPrefs.Save();
//             }
//         }

//         RefreshUI();
//     }

//     public void EquipCurrentItem()
//     {
//         if (_currentCategory == ShopCategory.Bird)
//         {
//             if (!IsBirdUnlocked(_currentBirdIndex)) return;

//             PlayerPrefs.SetInt(SELECTED_BIRD_KEY, _currentBirdIndex);
//             _playerSpriteRenderer.sprite = _birdSprites[_currentBirdIndex];
//         }
//         else
//         {
//             if (!IsBackgroundUnlocked(_currentBackgroundIndex)) return;

//             PlayerPrefs.SetInt(SELECTED_BG_KEY, _currentBackgroundIndex);

//             if (_backgroundRenderer != null)
//             {
//                 _backgroundRenderer.sprite = _backgroundItems[_currentBackgroundIndex].backgroundSprite;
//             }
//         }

//         PlayerPrefs.Save();
//         RefreshUI();
//     }

//     // =========================
//     // LOAD SAVED DATA
//     // =========================
//     private void LoadSelectedBird()
//     {
//         int selectedBird = PlayerPrefs.GetInt(SELECTED_BIRD_KEY, 0);
//         if (selectedBird >= 0 && selectedBird < _birdSprites.Length)
//         {
//             _playerSpriteRenderer.sprite = _birdSprites[selectedBird];
//         }

//         // Bird mặc định index 0 luôn unlocked
//         PlayerPrefs.SetInt(GetBirdUnlockKey(0), 1);
//     }

//     private void LoadSelectedBackground()
//     {
//         if (_backgroundItems == null || _backgroundItems.Length == 0 || _backgroundRenderer == null)
//             return;

//         // Background đầu tiên mặc định mở sẵn
//         PlayerPrefs.SetInt(GetBackgroundUnlockKey(0), 1);

//         int selectedBg = PlayerPrefs.GetInt(SELECTED_BG_KEY, 0);
//         if (selectedBg >= 0 && selectedBg < _backgroundItems.Length)
//         {
//             _backgroundRenderer.sprite = _backgroundItems[selectedBg].backgroundSprite;
//         }
//     }

//     // =========================
//     // UI REFRESH
//     // =========================
//     private void RefreshUI()
//     {
//         if (_currentCategory == ShopCategory.Bird)
//         {
//             _categoryText.text = "BIRD SHOP";

//             _previewImage.sprite = _birdSprites[_currentBirdIndex];
//             _itemNameText.text = "Bird #" + _currentBirdIndex;
//             _priceText.text = _birdPrices[_currentBirdIndex].ToString();

//             bool unlocked = IsBirdUnlocked(_currentBirdIndex);
//             bool equipped = IsSelectedBird(_currentBirdIndex);

//             _buyButton.gameObject.SetActive(!unlocked);
//             _equipButton.gameObject.SetActive(unlocked);

//             _buyButtonText.text = "Buy";
//             _equipButtonText.text = equipped ? "Equipped" : "Equip";
//             _equipButton.interactable = !equipped;

//             _statusText.text = unlocked ? (equipped ? "Owned / Equipped" : "Owned") : "Locked";
//         }
//         else
//         {
//             _categoryText.text = "BACKGROUND SHOP";

//             _previewImage.sprite = _backgroundItems[_currentBackgroundIndex].previewSprite;
//             _itemNameText.text = string.IsNullOrEmpty(_backgroundItems[_currentBackgroundIndex].id)
//                 ? "Background #" + _currentBackgroundIndex
//                 : _backgroundItems[_currentBackgroundIndex].id;

//             _priceText.text = _backgroundItems[_currentBackgroundIndex].price.ToString();

//             bool unlocked = IsBackgroundUnlocked(_currentBackgroundIndex);
//             bool equipped = IsSelectedBackground(_currentBackgroundIndex);

//             _buyButton.gameObject.SetActive(!unlocked);
//             _equipButton.gameObject.SetActive(unlocked);

//             _buyButtonText.text = "Buy";
//             _equipButtonText.text = equipped ? "Equipped" : "Equip";
//             _equipButton.interactable = !equipped;

//             _statusText.text = unlocked ? (equipped ? "Owned / Equipped" : "Owned") : "Locked";
//         }
//     }

//     // =========================
//     // HELPERS
//     // =========================
//     private string GetBirdUnlockKey(int index) => "BirdUnlocked_" + index;
//     private string GetBackgroundUnlockKey(int index) => "BackgroundUnlocked_" + index;

//     private bool IsBirdUnlocked(int index)
//     {
//         if (index == 0) return true;
//         return PlayerPrefs.GetInt(GetBirdUnlockKey(index), 0) == 1;
//     }

//     private bool IsBackgroundUnlocked(int index)
//     {
//         if (index == 0) return true;
//         return PlayerPrefs.GetInt(GetBackgroundUnlockKey(index), 0) == 1;
//     }

//     private bool IsSelectedBird(int index)
//     {
//         return PlayerPrefs.GetInt(SELECTED_BIRD_KEY, 0) == index;
//     }

//     private bool IsSelectedBackground(int index)
//     {
//         return PlayerPrefs.GetInt(SELECTED_BG_KEY, 0) == index;
//     }
// }