using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int _coinValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        deib
        if (!collision.CompareTag("Player")) return;

        // if (CoinWallet.Instance != null)
        // {
        //     CoinWallet.Instance.AddCoins(_coinValue);
        // }

        Destroy(gameObject);
    }
}