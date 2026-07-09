using UnityEngine;

public class PipeIncreaseScore : MonoBehaviour
{
    [Header("Coin Spawn")]
    [SerializeField] private GameObject _coinPrefab; // NEW
    [SerializeField][Range(0f, 1f)] private float _coinSpawnChance = 0.35f; // NEW
    [SerializeField] private Transform _coinSpawnPoint; // NEW
    [SerializeField] private float _randomYRange = 0.5f; // NEW

    private bool _hasTriggered = false; // NEW

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasTriggered) return; // NEW
        if (!collision.gameObject.CompareTag("Player")) return;

        _hasTriggered = true; // NEW

        Score.instance.UpdateScore();

        // NEW: random coin spawn sau khi qua cột
        TrySpawnCoin();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterPipePassed();
        }
    }

    private void TrySpawnCoin() // NEW
    {
        if (_coinPrefab == null) return;

        float roll = Random.value;
        if (roll > _coinSpawnChance) return;

        Vector3 spawnPos;

        if (_coinSpawnPoint != null)
        {
            spawnPos = _coinSpawnPoint.position;
        }
        else
        {
            spawnPos = transform.position + new Vector3(1.0f, Random.Range(-_randomYRange, _randomYRange), 0f);
        }

        Instantiate(_coinPrefab, spawnPos, Quaternion.identity);
    }
}