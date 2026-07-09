using UnityEngine;
using UnityEngine.InputSystem;

public class FlyBehavior : MonoBehaviour
{
    [Header("Fly Settings")]
    [SerializeField] private float _velocity = 1.5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Anti Spam")]
    [Tooltip("Khoảng nghỉ rất ngắn giữa 2 lần vỗ cánh.")]
    [SerializeField] private float _flapCooldown = 0.08f;

    [Header("Screen Boundary")]
    [Tooltip("Camera chính của game. Có thể để trống để tự tìm Main Camera.")]
    [SerializeField] private Camera _gameCamera;

    [Tooltip("Collider của chim. Có thể để trống để tự tìm.")]
    [SerializeField] private Collider2D _birdCollider;

    [Tooltip("Khoảng cách nhỏ trước mép màn hình để chim chết sớm hơn một chút.")]
    [SerializeField, Min(0f)] private float _screenEdgePadding = 0.02f;

    private Rigidbody2D _rb;
    private float _nextFlapTime;
    private bool _isDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_gameCamera == null)
        {
            _gameCamera = Camera.main;
        }

        if (_birdCollider == null)
        {
            _birdCollider = GetComponent<Collider2D>();

            if (_birdCollider == null)
            {
                _birdCollider = GetComponentInChildren<Collider2D>();
            }
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || _isDead || _rb == null)
        {
            return;
        }

        CheckScreenBoundary();

        if (_isDead)
        {
            return;
        }

        if (IsFlyInputPressed() && Time.time >= _nextFlapTime)
        {
            _nextFlapTime = Time.time + _flapCooldown;

            _rb.velocity = new Vector2(
                _rb.velocity.x,
                _velocity
            );

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFly();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null || _isDead)
        {
            return;
        }

        float rotationZ =
            _rb.velocity.y * _rotationSpeed;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            rotationZ
        );
    }

    private bool IsFlyInputPressed()
    {
        bool mousePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool touchPressed =
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        bool gamepadPressed =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        return mousePressed ||
               touchPressed ||
               gamepadPressed;
    }

    private void CheckScreenBoundary()
    {
        if (_gameCamera == null ||
            _birdCollider == null)
        {
            return;
        }

        float distanceFromCamera = Mathf.Abs(
            transform.position.z -
            _gameCamera.transform.position.z
        );

        float screenTop =
            _gameCamera.ViewportToWorldPoint(
                new Vector3(
                    0.5f,
                    1f,
                    distanceFromCamera
                )
            ).y;

        float screenBottom =
            _gameCamera.ViewportToWorldPoint(
                new Vector3(
                    0.5f,
                    0f,
                    distanceFromCamera
                )
            ).y;

        Bounds birdBounds = _birdCollider.bounds;

        bool touchedTop =
            birdBounds.max.y >=
            screenTop - _screenEdgePadding;

        bool touchedBottom =
            birdBounds.min.y <=
            screenBottom + _screenEdgePadding;

        if (touchedTop || touchedBottom)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        Die();
    }

    private void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy GameManager.instance."
            );
        }
    }
}