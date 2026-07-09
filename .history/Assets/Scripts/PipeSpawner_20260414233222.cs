using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float _maxtime = 1.5f;
    [SerializeField] private float _heightRange = 0.45f;
    [SerializeField] private GameObject _pipe;

    [Header("Collectible Spawn")]
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _gemPrefab;
    [SerializeField] [Range(0f, 1f)] private float _collectibleSpawnChance = 0.35f;
    [SerializeField] [Range(0f, 1f)] private float _gemChanceInsteadOfCoin = 0.15f;

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

        TrySpawnCollectible(pipe);
    }

    private void TrySpawnCollectible(GameObject pipe)
    {
        if (Random.value > _collectibleSpawnChance) return;

        CoinSpawnPos spawnPoint = pipe.GetComponentInChildren<CoinSpawnPos>();
        if (spawnPoint == null) return;

        bool spawnGem = _gemPrefab != null && Random.value < _gemChanceInsteadOfCoin;
        GameObject prefabToSpawn = spawnGem ? _gemPrefab : _coinPrefab;

        if (prefabToSpawn == null) return;

        GameObject collectible = Instantiate(prefabToSpawn, spawnPoint.transform.position, Quaternion.identity, pipe.transform);
        Destroy(collectible, 10f);
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