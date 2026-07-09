using TMPro;
using UnityEngine;

public class CoinWallet : MonoBehaviour
{
    public static CoinWallet Instance;

    [SerializeField] private TextMeshProUGUI _coinText;

    private int _coins;
    private const string COIN_KEY = "PLAYER_COINS";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        _coins = PlayerPrefs.GetInt(COIN_KEY, 0);
        RefreshUI();
    }

    public void AddCoins(int amount)
    {
        _coins += amount;
        if (_coins < 0) _coins = 0;

        PlayerPrefs.SetInt(COIN_KEY, _coins);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public bool TrySpendCoins(int amount)
    {
        if (_coins < amount)
            return false;

        _coins -= amount;
        PlayerPrefs.SetInt(COIN_KEY, _coins);
        PlayerPrefs.Save();
        RefreshUI();
        return true;
    }

    public int GetCoins()
    {
        return _coins;
    }

    private void RefreshUI()
    {
        if (_coinText != null)
        {
            _coinText.text = _coins.ToString();
        }
    }
}