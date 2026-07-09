using UnityEngine;

public class LoopGround : MonoBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _width = 6f;

    private SpriteRenderer _spriteRendere;
    private float _startPos;

    private void Start()
    {
        _spriteRendere = GetComponent<SpriteRenderer>();
        _startPos = transform.position.x;
        RefreshVisualBounds(); // CHANGED
    }

    private void Update()
    {
        transform.position = new Vector2(transform.position.x + _speed * Time.deltaTime, transform.position.y);

        if (transform.position.x < _startPos - _width / 2f)
        {
            transform.position = new Vector2(_startPos, transform.position.y);
        }
    }

    public void RefreshVisualBounds() // NEW
    {
        if (_spriteRendere == null)
            _spriteRendere = GetComponent<SpriteRenderer>();

        _width = _spriteRendere.bounds.size.x;
    }
}