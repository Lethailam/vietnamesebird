private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log("Coin touched by: " + collision.name);

    if (CoinWallet.Instance != null)
    {
        CoinWallet.Instance.AddCoins(_coinValue);
    }

    Destroy(gameObject);
}