using System.Collections;
using System.Collections.Generic;
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
    
    private void SpawnPipe()
    {
        Vector3 spawnPos = transform.position + new Vector3(0, Random.Range(-_heightRange, _heightRange));
        GameObject pipe = Instantiate(_pipe, spawnPos, Quaternion.identity);

        Destroy
    }
}
