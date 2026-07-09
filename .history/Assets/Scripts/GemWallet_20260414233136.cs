using TMPro;
using UnityEngine;

public class GemWallet : MonoBehaviour
{
    public static GemWallet Instance;

    [SerializeField] private TextMeshProUGUI _gemText;

    private int _gems;
    private const string GEM_KEY = "PLAYER_GEMS";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        _gems = PlayerPrefs.GetInt(GEM_KEY, 0);
        RefreshUI();
    }

    public void AddGem(int amount = 1)
    {
        _gems += amount;
        if (_gems < 0) _gems = 0;

        PlayerPrefs.SetInt(GEM_KEY, _gems);
        PlayerPrefs.Save();
        RefreshUI();
    }

    public bool TrySpendGem(int amount = 1)
    {
        if (_gems < amount)
            return false;

        _gems -= amount;
        PlayerPrefs.SetInt(GEM_KEY, _gems);
        PlayerPrefs.Save();
        RefreshUI();
        return true;
    }

    public int GetGems()
    {
        return _gems;
    }

    private void RefreshUI()
    {
        if (_gemText != null)
            _gemText.text = _gems.ToString();
    }
}