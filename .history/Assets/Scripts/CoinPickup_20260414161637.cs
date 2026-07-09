using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int _coinValue = 1;

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (!collision.CompareTag("Player")) return;

    //     if (CoinWallet.Instance != null)
    //     {
    //         CoinWallet.Instance.AddCoins(_coinValue);
    //     }

    //     Destroy(gameObject);
    // }
    private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log("Coin touched by: " + collision.name);

    if (CoinWallet.Instance != null)
    {
        CoinWallet.Instance.AddCoins(_coinValue);
    }

    Destroy(gameObject);
}
}