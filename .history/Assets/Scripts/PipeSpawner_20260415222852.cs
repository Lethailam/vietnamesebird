using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float _maxtime = 1.5f;
    [SerializeField] private float _heightRange = 0.45f;
    [SerializeField] private GameObject _pipe;

    [Header("Coin Spawn")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] [Range(0f, 1f)] private float _coinSpawnChance = 0.35f;

    private float _timer;

    private void Start()
    {
        SpawnPipe();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (_timer > _maxtime)
        {
            SpawnPipe();
            _timer = 0f;
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }

    private void SpawnPipe()
    {
        Vector3 spawnPos = transform.position + new Vector3(0f, Random.Range(-_heightRange, _heightRange), 0f);

        GameObject pipe = Instantiate(_pipe, spawnPos, Quaternion.identity);
        Destroy(pipe, 10f);

        TrySpawnCoin(pipe);
    }

    private void TrySpawnCoin(GameObject pipe)
    {
        if (_coinPrefab == null) return;
        if (Random.value > _coinSpawnChance) return;

        CoinSpawnPos spawnPoint = pipe.GetComponentInChildren<CoinSpawnPos>();
        if (spawnPoint == null) return;

        GameObject coin = Instantiate(_coinPrefab, spawnPoint.transform.position, Quaternion.identity, pipe.transform);
        Destroy(coin, 10f);
    }

    public void SetPipePrefab(GameObject newPipe)
    {
        _pipe = newPipe;
    }

    public void ResetSpawner(bool spawnImmediately = false)
    {
        _timer = 0f;

        if (spawnImmediately)
        {
            SpawnPipe();
        }
    }
}