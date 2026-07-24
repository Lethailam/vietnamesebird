using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlyBehavior : MonoBehaviour
{
    [Header("Fly Settings")]
    [SerializeField] private float _velocity = 1.5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Wing Animation")]
    [SerializeField] private BirdWingAnimator _wingAnimator;


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
    [Tooltip("Chim văng nhẹ ra sau bao xa. Giá trị nhỏ thôi, ví dụ 0.03 - 0.08.")]
    [SerializeField] private float _hitPushDistance = 0.05f;

    [Tooltip("Thời gian chim văng nhẹ ra sau.")]
    [SerializeField] private float _hitPushDuration = 0.10f;

    [Tooltip("Tốc độ rơi ban đầu sau khi va chạm.")]
    [SerializeField] private float _fallStartSpeed = 0.55f;

    [Tooltip("Gia tốc rơi. Càng nhỏ thì rơi càng từ từ.")]
    [SerializeField] private float _fallAcceleration = 1.1f;

    [Tooltip("Tốc độ rơi tối đa.")]
    [SerializeField] private float _fallMaxSpeed = 1.8f;

    [Tooltip("Tốc độ xoay tròn khi chim đang rơi.")]
    [SerializeField] private float _fallSpinSpeed = 420f;

    [Tooltip("Góc nằm lại của chim sau khi chạm ground.")]
    [SerializeField] private float _landRotationZ = -90f;

    [Tooltip("Sau khi chim nằm trên ground, chờ thêm một chút rồi hiện Game Over.")]
    [SerializeField] private float _gameOverExtraDelay = 0.35f;

    [Tooltip("Điểm mặt đất nơi chim sẽ dừng lại. Nên kéo GroundStopPoint vào đây.")]
    [SerializeField] private Transform _groundStopPoint;

    [Tooltip("Tinh chỉnh độ cao khi chim nằm trên ground.")]
    [SerializeField] private float _groundYOffset = 0.03f;

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

        if (_wingAnimator == null)
        {
            _wingAnimator = GetComponent<BirdWingAnimator>();

            if (_wingAnimator == null)
            {
                _wingAnimator =
                    GetComponentInChildren<BirdWingAnimator>();
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

            if (_wingAnimator != null)
            {
                _wingAnimator.FlapOnce();
            }
        }
    }

    private void Start()
{
    ApplySelectedBirdSkin();
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

        if (_wingAnimator != null)
        {
            _wingAnimator.Stop();
        }

        Debug.Log(
            "BIRD: Va chạm, dừng gameplay, văng nhẹ, xoay tròn và rơi xuống ground."
        );

        if (_freezeGameplayWhenDead)
        {
            FreezeGameplayObjects();
        }

        if (_rb != null)
        {
            /*
             * Tắt Rigidbody để chim không bị lực vật lý
             * làm văng xa khỏi khung hình.
             * Phần rơi và xoay sẽ được điều khiển thủ công bằng Coroutine.
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
                PushBackSpinFallToGround(
                    birdHalfHeight
                )
            );
    }

    private IEnumerator PushBackSpinFallToGround(
        float birdHalfHeight
    )
    {
        Vector3 startPosition =
            transform.position;

        /*
         * Chim đang đứng gần bên trái màn hình,
         * cột đi từ phải sang trái.
         * Khi va chạm, cho chim bật nhẹ về bên trái một chút.
         */
        Vector3 pushedPosition =
            startPosition +
            new Vector3(
                -_hitPushDistance,
                0f,
                0f
            );

        float elapsedPush = 0f;

        /*
         * Giai đoạn 1:
         * Chim văng nhẹ ra vài pixel.
         */
        while (elapsedPush < _hitPushDuration)
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            elapsedPush += deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedPush / _hitPushDuration
                );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    pushedPosition,
                    smoothT
                );

            /*
             * Xoay rất nhẹ ngay lúc bật ra.
             */
            transform.Rotate(
                0f,
                0f,
                _fallSpinSpeed * 0.35f * deltaTime
            );

            yield return null;
        }

        float fallSpeed =
            _fallStartSpeed;

        float landingY =
            GetLandingY(birdHalfHeight);

        /*
         * Giai đoạn 2:
         * Chim xoay tròn trong lúc rơi.
         * Không cho rơi xuyên qua ground.
         */
        while (transform.position.y > landingY)
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
             * Giữ X cố định sau cú văng nhẹ.
             * Như vậy chim không biến mất khỏi khung hình.
             */
            currentPosition.x =
                pushedPosition.x;

            /*
             * Chặn chim lại tại mặt ground.
             */
            if (currentPosition.y <= landingY)
            {
                currentPosition.y = landingY;
            }

            transform.position =
                currentPosition;

            /*
             * Xoay tròn trong lúc rơi.
             */
            transform.Rotate(
                0f,
                0f,
                _fallSpinSpeed * deltaTime
            );

            yield return null;
        }

        /*
         * Giai đoạn 3:
         * Chim nằm lại trên ground.
         */
        transform.position =
            new Vector3(
                pushedPosition.x,
                landingY,
                transform.position.z
            );

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                _landRotationZ
            );

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

    private float GetLandingY(
        float birdHalfHeight
    )
    {
        /*
         * Cách chuẩn nhất:
         * Tạo Empty Object tên GroundStopPoint,
         * đặt tại mặt trên của ground,
         * rồi kéo vào ô Ground Stop Point trong Inspector.
         */
        if (_groundStopPoint != null)
        {
            return _groundStopPoint.position.y +
                   birdHalfHeight +
                   _groundYOffset;
        }

        /*
         * Nếu chưa gán GroundStopPoint,
         * tạm lấy mép dưới màn hình làm điểm dừng.
         */
        if (_gameCamera == null)
        {
            return transform.position.y;
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

        return screenBottom +
               birdHalfHeight +
               _groundYOffset;
    }

    private void FreezeGameplayObjects()
    {
        /*
         * Không dùng Time.timeScale = 0 ở đây.
         * Vì nếu Time.timeScale = 0 thì mọi thứ bị pause cứng.
         *
         * Ta chỉ tắt các script làm pipe/ground di chuyển.
         * Chim vẫn rơi bằng Coroutine dùng Time.unscaledDeltaTime.
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

    private void ApplySelectedBirdSkin()
    {
        if (_wingAnimator == null)
        {
            return;
        }

        if (BirdSkinManager.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy BirdSkinManager.Instance."
            );

            return;
        }

        BirdSkinData selectedSkin =
            BirdSkinManager.Instance.GetSelectedSkin();

        if (selectedSkin == null)
        {
            Debug.LogWarning(
                "Không tìm thấy selected bird skin."
            );

            return;
        }

        _wingAnimator.SetWingSprites(
            selectedSkin.wingUpSprite,
            selectedSkin.wingMidSprite,
            selectedSkin.wingDownSprite,
            true
        );
    }
}