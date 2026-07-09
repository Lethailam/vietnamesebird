using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopGround : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _resetPositionX = -10f;
    [SerializeField] private float _moveAheadX = 20f;

    private void Update()
    {
        transform.position += Vector3.left * _speed * Time.deltaTime;

        if (transform.position.x <= _resetPositionX)
        {
            transform.position += new Vector3(_moveAheadX, 0f, 0f);
        }
    }
}
