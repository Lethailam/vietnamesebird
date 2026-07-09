using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private float _maxtime = 1.5f;
    [SerializeField] private float _heightRange = 0.45f;
    [SerializeField] private GameObject _pipe;

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
        Vector3 spawnPos = transform.position + new Vector3(0, Random.Range(-_heightRange, _heightRange), 0);
        GameObject pipe = Instantiate(_pipe, spawnPos, Quaternion.identity);
        Destroy(pipe, 10f);
    }

    public void SetPipePrefab(GameObject newPipe)
    {
        _pipe = newPipe;
    }

    public void ResetSpawner(bool spawnImmediately = false) // NEW
    {
        _timer = 0f;

        if (spawnImmediately)
        {
            SpawnPipe();
        }
    }
}