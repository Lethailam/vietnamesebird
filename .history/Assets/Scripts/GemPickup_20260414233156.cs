using UnityEngine;

public class GemPickup : MonoBehaviour
{
    private bool _collected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_collected) return;
        if (!collision.CompareTag("Player")) return;

        _collected = true;

        if (GemWallet.Instance != null)
        {
            GemWallet.Instance.AddGem(1);
        }

        Destroy(gameObject);
    }
}