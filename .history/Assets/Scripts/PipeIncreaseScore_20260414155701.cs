using UnityEngine;

public class PipeIncreaseScore : MonoBehaviour
{
    [Header("Coin Spawn")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] [Range(0f, 1f)] private float _coinSpawnChance = 0.35f;
    [SerializeField] private Transform _coinSpawnPoint;
    [SerializeField] private float _randomYRange = 0.5f;

    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasTriggered) return;
        if (!collision.CompareTag("Player")) return;

        _hasTriggered = true;

        Score.instance.UpdateScore();

        TrySpawnCoin();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterPipePassed();
        }
    }

    private void TrySpawnCoin()
    {
        if (_coinPrefab == null) return;
        if (Random.value > _coinSpawnChance) return;

        Vector3 spawnPos;

        if (_coinSpawnPoint != null)
        {
            spawnPos = _coinSpawnPoint.position;
        }
        else
        {
            spawnPos = transform.position + new Vector3(1f, Random.Range(-_randomYRange, _randomYRange), 0f);
        }

        Instantiate(_coinPrefab, spawnPos, Quaternion.identity);
    }
}