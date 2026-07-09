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
        float height = Random.Range(-_heightRange, _heightRange);
        Instantiate(_pipe, new Vector3(transform.position.x, transform.position.y + height, 0), Quaternion.identity);
    }
}
