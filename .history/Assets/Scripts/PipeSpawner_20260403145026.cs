using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
   [SerializeField] private GameObject _pipePrefab;
   [SerializeField] private float _spawnInterval = 2f;
   [SerializeField] private float _pipeHeight = 10f;
}
