using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopGround : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _width = 6f;

    private SpriteRenderer _spriteRendere;

    private Vector2 _startSize;
    private float _startPos;

    private void Start()
    {
        _spriteRendere = GetComponent<SpriteRenderer>();
        _startPos = transform.position.x;
        _width = GetComponent<BoxCollider2D>().size.x;
        _startSize = new Vector2(_spriteRendere.size.x, _spriteRendere.size.y);
    }

    private void Update()
    {
        transform.position = new Vector2(transform.position.x + _speed * Time.deltaTime, transform.position.y);

        if (transform.position.x < _startPos - )
        {
            transform.position = new Vector2(_startPos, transform.position.y);
        }
    }
}
