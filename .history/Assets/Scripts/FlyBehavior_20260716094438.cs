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
    [Tooltip("Lực bật ngang khi chim va chạm. Nên để nhỏ để chim không văng quá xa.")]
    [SerializeField] private float _deathBounceForceX = -0.5f;

    [Tooltip("Lực bật nhẹ lên khi chim va chạm.")]
    [SerializeField] private float _deathBounceForceY = 2.2f;

    [Tooltip("Tốc độ xoay tròn khi chim chóng mặt.")]
    [SerializeField] private float _deathSpinSpeed = 720f;

    [Tooltip("Trọng lực khi chim rơi sau va chạm.")]
    [SerializeField] private float _deathGravityScale = 2.8f;

    [Tooltip("Sau bao lâu thì hiện Game Over Panel.")]
    [SerializeField] private float _gameOverDelay = 1.4f;

    [Tooltip("Tắt collider sau khi va chạm để chim không va đập liên tục.")]
    [SerializeField] private bool _disableColliderAfterDeath = true;

    [Header("Freeze Gameplay On Death")]
    [Tooltip("Dừng toàn bộ pipe, spawner, ground khi chim chết để người chơi nhìn thấy khoảnh khắc chim rơi.")]
    [SerializeField] private bool _freezeGameplayWhenDead = true;

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
         * Chỉ dùng nếu pipe của bạn đang bật Is Trigger.
         * Vùng cộng điểm không được đặt tag Pipe hoặc Obstacle.
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
            "BIRD: Va chạm, dừng gameplay và bắt đầu hiệu ứng rơi."
        );

        if (_freezeGameplayWhenDead)
        {
            FreezeGameplayObjects();
        }

        if (_rb != null)
        {
            _rb.simulated = true;
            _rb.gravityScale = _deathGravityScale;

            // Xóa vận tốc bay cũ.
            _rb.velocity = Vector2.zero;

            // Bật nhẹ ra một chút, không văng quá xa.
            _rb.velocity = new Vector2(
                _deathBounceForceX,
                _deathBounceForceY
            );

            // Xoay tròn.
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

    private void FreezeGameplayObjects()
    {
        /*
         * Không dùng Time.timeScale = 0 ở đây.
         * Vì nếu Time.timeScale = 0 thì chim cũng không rơi nữa.
         *
         * Ta chỉ tắt các script làm pipe, ground, spawner di chuyển.
         */

        PipeSpawner[] pipeSpawners =
            FindObjectsOfType<PipeSpawner>();

        for (int i = 0; i < pipeSpawners.Length; i++)
        {
            pipeSpawners[i].enabled = false;
        }

        MovePipe[] movePipes =
            FindObjectsOfType<MovePipe>();

        for (int i = 0; i < movePipes.Length; i++)
        {
            movePipes[i].enabled = false;
        }

        LoopGround[] loopGrounds =
            FindObjectsOfType<LoopGround>();

        for (int i = 0; i < loopGrounds.Length; i++)
        {
            loopGrounds[i].enabled = false;
        }

        /*
         * Nếu có object nào đang di chuyển bằng Rigidbody2D,
         * dừng vận tốc của nó lại.
         * Không dừng Rigidbody2D của chính con chim.
         */
        Rigidbody2D[] allRigidbodies =
            FindObjectsOfType<Rigidbody2D>();

        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i] == _rb)
            {
                continue;
            }

            allRigidbodies[i].velocity = Vector2.zero;
            allRigidbodies[i].angularVelocity = 0f;
        }
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