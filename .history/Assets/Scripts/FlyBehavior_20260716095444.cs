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

    [Header("Death Fall Effect")]
    [Tooltip("Chim văng nhẹ ra sau bao xa. Giá trị nhỏ thôi, ví dụ 0.08 - 0.18.")]
    [SerializeField] private float _hitPushDistance = 0.12f;

    [Tooltip("Thời gian chim văng nhẹ ra sau.")]
    [SerializeField] private float _hitPushDuration = 0.12f;

    [Tooltip("Tốc độ rơi ban đầu sau khi va chạm.")]
    [SerializeField] private float _fallStartSpeed = 0.8f;

    [Tooltip("Gia tốc rơi. Càng nhỏ thì rơi càng từ từ.")]
    [SerializeField] private float _fallAcceleration = 1.8f;

    [Tooltip("Tốc độ rơi tối đa.")]
    [SerializeField] private float _fallMaxSpeed = 2.6f;

    [Tooltip("Góc nghiêng của chim khi rơi xuống.")]
    [SerializeField] private float _fallRotationZ = -35f;

    [Tooltip("Tốc độ nghiêng từ từ sang trạng thái rơi.")]
    [SerializeField] private float _rotateToFallSpeed = 8f;

    [Tooltip("Sau khi chim chạm đáy, chờ thêm một chút rồi hiện Game Over.")]
    [SerializeField] private float _gameOverExtraDelay = 0.15f;

    [Tooltip("Tắt collider sau khi va chạm để chim không va đập liên tục.")]
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
            DieWithFallEffect();
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

        DieWithFallEffect();
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
         * Chỉ dùng nếu cột của bạn là Trigger.
         * Vùng cộng điểm không được đặt tag Pipe hoặc Obstacle.
         */
        if (other.CompareTag("Pipe") ||
            other.CompareTag("Obstacle"))
        {
            DieWithFallEffect();
        }
    }

    private void DieWithFallEffect()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        Debug.Log(
            "BIRD: Va chạm, văng nhẹ ra rồi rơi xuống."
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
             */
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }

        float birdHalfHeight = 0.15f;

        if (_birdCollider != null)
        {
            birdHalfHeight =
                _birdCollider.bounds.extents.y;
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
                PushBackThenFallToGround(
                    birdHalfHeight
                )
            );
    }

    private IEnumerator PushBackThenFallToGround(
        float birdHalfHeight
    )
    {
        Vector3 startPosition =
            transform.position;

        /*
         * Chim đang bay kiểu Flappy từ trái qua phải,
         * cột đi từ phải sang trái.
         * Khi va chạm, cho chim bật nhẹ về bên trái.
         */
        Vector3 pushedPosition =
            startPosition +
            new Vector3(
                -_hitPushDistance,
                0f,
                0f
            );

        float elapsedPush = 0f;

        while (elapsedPush < _hitPushDuration)
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            elapsedPush += deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedPush / _hitPushDuration
                );

            /*
             * SmoothStep giúp cú bật nhẹ mềm hơn,
             * không bị giật.
             */
            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    pushedPosition,
                    smoothT
                );

            transform.rotation =
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(
                        0f,
                        0f,
                        _fallRotationZ
                    ),
                    deltaTime * _rotateToFallSpeed
                );

            yield return null;
        }

        float fallSpeed =
            _fallStartSpeed;

        while (!HasTouchedBottom(birdHalfHeight))
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            fallSpeed +=
                _fallAcceleration * deltaTime;

            fallSpeed =
                Mathf.Min(
                    fallSpeed,
                    _fallMaxSpeed
                );

            Vector3 currentPosition =
                transform.position;

            currentPosition.y -=
                fallSpeed * deltaTime;

            /*
             * Giữ X gần như cố định sau cú văng nhẹ.
             * Như vậy chim sẽ không biến mất khỏi khung hình.
             */
            currentPosition.x =
                pushedPosition.x;

            transform.position =
                currentPosition;

            transform.rotation =
                Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(
                        0f,
                        0f,
                        _fallRotationZ
                    ),
                    deltaTime * _rotateToFallSpeed
                );

            yield return null;
        }

        if (_gameOverExtraDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                _gameOverExtraDelay
            );
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

    private bool HasTouchedBottom(
        float birdHalfHeight
    )
    {
        if (_gameCamera == null)
        {
            return false;
        }

        float distanceFromCamera = Mathf.Abs(
            transform.position.z -
            _gameCamera.transform.position.z
        );

        float screenBottom =
            _gameCamera.ViewportToWorldPoint(
                new Vector3(
                    0.5f,
                    0f,
                    distanceFromCamera
                )
            ).y;

        float birdBottom =
            transform.position.y - birdHalfHeight;

        return birdBottom <=
               screenBottom + _screenEdgePadding;
    }

    private void FreezeGameplayObjects()
    {
        /*
         * Không dùng Time.timeScale = 0 tại đây.
         * Ta chỉ dừng các object gameplay.
         * Chim vẫn rơi bằng Coroutine dùng unscaledDeltaTime.
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

            allRigidbodies[i].velocity =
                Vector2.zero;

            allRigidbodies[i].angularVelocity =
                0f;
        }
    }
}