using System.Collections;
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

    [Header("Death Effect")]
    [Tooltip("Lực bật ngang khi chim va chạm. Giá trị âm nghĩa là bật về bên trái.")]
    [SerializeField] private float _deathBounceForceX = -2.5f;

    [Tooltip("Lực bật lên khi chim va chạm.")]
    [SerializeField] private float _deathBounceForceY = 4.5f;

    [Tooltip("Tốc độ xoay tròn khi chim chóng mặt.")]
    [SerializeField] private float _deathSpinSpeed = 900f;

    [Tooltip("Trọng lực khi chim rơi sau va chạm.")]
    [SerializeField] private float _deathGravityScale = 2.5f;

    [Tooltip("Sau bao lâu thì hiện Game Over Panel.")]
    [SerializeField] private float _gameOverDelay = 1.2f;

    [Tooltip("Tắt collider sau khi va chạm để chim không bị va đập liên tục.")]
    [SerializeField] private bool _disableColliderAfterDeath = true;

    private Rigidbody2D _rb;
    private float _nextFlapTime;
    private bool _isDead;
    private Coroutine _gameOverCoroutine;

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
        if (Time.timeScale == 0f ||
            _isDead ||
            _rb == null)
        {
            return;
        }

        CheckScreenBoundary();

        if (_isDead)
        {
            return;
        }

        if (IsFlyInputPressed() &&
            Time.time >= _nextFlapTime)
        {
            _nextFlapTime =
                Time.time + _flapCooldown;

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
            DieWithEffect();
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (_isDead)
        {
            return;
        }

        DieWithEffect();
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (_isDead)
        {
            return;
        }

        /*
         * Chỉ dùng đoạn này nếu cột của bạn đang bật Is Trigger.
         * Score trigger không nên đặt tag Pipe hoặc Obstacle.
         */
        if (other.CompareTag("Pipe") ||
            other.CompareTag("Obstacle"))
        {
            DieWithEffect();
        }
    }

    private void DieWithEffect()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        Debug.Log(
            "BIRD: Va chạm, bắt đầu hiệu ứng bật ra và rơi xuống."
        );

        if (_rb != null)
        {
            _rb.simulated = true;

            // Cho chim rơi nhanh hơn sau khi va chạm.
            _rb.gravityScale = _deathGravityScale;

            // Xóa vận tốc cũ để hiệu ứng bật ra rõ ràng hơn.
            _rb.velocity = Vector2.zero;

            // Bật ngược ra rồi rơi xuống.
            _rb.velocity = new Vector2(
                _deathBounceForceX,
                _deathBounceForceY
            );

            // Xoay tròn chóng mặt.
            _rb.angularVelocity = _deathSpinSpeed;
        }

        if (_disableColliderAfterDeath &&
            _birdCollider != null)
        {
            _birdCollider.enabled = false;
        }

        if (_gameOverCoroutine != null)
        {
            StopCoroutine(_gameOverCoroutine);
        }

        _gameOverCoroutine =
            StartCoroutine(
                ShowGameOverAfterDelay()
            );
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            _gameOverDelay
        );

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