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

    [Header("Death Leaf Fall Effect")]
    [Tooltip("Sau bao lâu thì hiện Game Over Panel.")]
    [SerializeField] private float _gameOverDelay = 1.45f;

    [Tooltip("Tốc độ rơi ban đầu của chim sau khi va chạm.")]
    [SerializeField] private float _deathStartFallSpeed = 0.4f;

    [Tooltip("Gia tốc rơi. Càng lớn chim càng rơi nhanh.")]
    [SerializeField] private float _deathFallAcceleration = 3.2f;

    [Tooltip("Tốc độ rơi tối đa.")]
    [SerializeField] private float _deathMaxFallSpeed = 4.2f;

    [Tooltip("Độ lắc ngang như lá rơi. Để nhỏ để chim không bay khỏi màn hình.")]
    [SerializeField] private float _deathSwayAmplitude = 0.18f;

    [Tooltip("Tần suất lắc ngang.")]
    [SerializeField] private float _deathSwayFrequency = 5.5f;

    [Tooltip("Góc nghiêng tối đa khi rơi.")]
    [SerializeField] private float _deathTiltAngle = 35f;

    [Tooltip("Tần suất nghiêng qua lại.")]
    [SerializeField] private float _deathTiltFrequency = 6.5f;

    [Tooltip("Có tắt collider sau khi chết không.")]
    [SerializeField] private bool _disableColliderAfterDeath = true;

    [Header("Freeze Gameplay On Death")]
    [Tooltip("Dừng pipe, spawner, ground khi chim chết.")]
    [SerializeField] private bool _freezeGameplayWhenDead = true;

    private Rigidbody2D _rb;
    private float _nextFlapTime;
    private bool _isDead;
    private Coroutine _deathCoroutine;

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
            DieWithLeafFallEffect();
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

        DieWithLeafFallEffect();
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
         * Chỉ dùng khi pipe/cột là Trigger.
         * Vùng cộng điểm không được đặt tag Pipe hoặc Obstacle.
         */
        if (other.CompareTag("Pipe") ||
            other.CompareTag("Obstacle"))
        {
            DieWithLeafFallEffect();
        }
    }

    private void DieWithLeafFallEffect()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        Debug.Log(
            "BIRD: Va chạm, dừng gameplay và rơi kiểu lá rơi."
        );

        if (_freezeGameplayWhenDead)
        {
            FreezeGameplayObjects();
        }

        if (_rb != null)
        {
            /*
             * Tắt mô phỏng Rigidbody để chim không bị lực vật lý
             * làm văng xa khỏi khung hình.
             *
             * Sau đó ta tự điều khiển vị trí chim bằng Coroutine.
             */
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }

        if (_disableColliderAfterDeath &&
            _birdCollider != null)
        {
            _birdCollider.enabled = false;
        }

        if (_deathCoroutine != null)
        {
            StopCoroutine(_deathCoroutine);
        }

        _deathCoroutine =
            StartCoroutine(
                LeafFallThenGameOver()
            );
    }

    private IEnumerator LeafFallThenGameOver()
    {
        Vector3 startPosition = transform.position;
        float startX = startPosition.x;

        float elapsed = 0f;
        float fallSpeed = _deathStartFallSpeed;

        /*
         * Cho chim hơi nghiêng xuống ngay lúc va chạm,
         * không xoay vòng vòng.
         */
        while (elapsed < _gameOverDelay)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsed += deltaTime;

            fallSpeed +=
                _deathFallAcceleration * deltaTime;

            fallSpeed = Mathf.Min(
                fallSpeed,
                _deathMaxFallSpeed
            );

            Vector3 currentPosition =
                transform.position;

            /*
             * Lắc ngang nhẹ như lá rơi.
             * Biên độ nhỏ nên chim không bay khỏi màn hình.
             */
            float swayOffset =
                Mathf.Sin(
                    elapsed * _deathSwayFrequency
                ) * _deathSwayAmplitude;

            float newX =
                startX + swayOffset;

            float newY =
                currentPosition.y -
                fallSpeed * deltaTime;

            transform.position = new Vector3(
                newX,
                newY,
                currentPosition.z
            );

            /*
             * Nghiêng qua lại như lá rơi.
             * Không dùng angularVelocity để tránh xoay vòng quá nhanh.
             */
            float tiltZ =
                Mathf.Sin(
                    elapsed * _deathTiltFrequency
                ) * _deathTiltAngle;

            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                tiltZ
            );

            yield return null;
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

    private void FreezeGameplayObjects()
    {
        /*
         * Không dùng Time.timeScale = 0 ở đây.
         * Nếu Time.timeScale = 0 thì Coroutine rơi vẫn chạy bằng unscaled,
         * nhưng nhiều logic khác có thể bị ảnh hưởng.
         *
         * Ta chỉ tắt các script làm màn chơi di chuyển.
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
}