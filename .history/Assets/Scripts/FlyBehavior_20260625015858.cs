using UnityEngine;
using UnityEngine.InputSystem;

public class FlyBehavior : MonoBehaviour
{
    [Header("Fly Settings")]
    [SerializeField] private float _velocity = 1.5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Anti Spam / Out Of Screen Protection")]
    [Tooltip("Khoảng nghỉ rất ngắn giữa 2 lần vỗ cánh để người chơi không spam tap quá nhanh.")]
    [SerializeField] private float _flapCooldown = 0.08f;

    [Tooltip("Nếu chim bay cao hơn mốc này thì tính là chết.")]
    [SerializeField] private float _maxYPosition = 5.2f;

    [Tooltip("Nếu chim rơi thấp hơn mốc này thì tính là chết.")]
    [SerializeField] private float _minYPosition = -5.2f;

    private Rigidbody2D _rb;
    private float _nextFlapTime;
    private bool _isDead;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f || _isDead || _rb == null) return;

        CheckOutOfScreenDeath();

        if (_isDead) return;

        if (IsFlyInputPressed() && Time.time >= _nextFlapTime)
        {
            _nextFlapTime = Time.time + _flapCooldown;

            _rb.velocity = new Vector2(_rb.velocity.x, _velocity);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayFly();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        transform.rotation = Quaternion.Euler(0, 0, _rb.velocity.y * _rotationSpeed);
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

        return mousePressed || touchPressed || gamepadPressed;
    }

    private void CheckOutOfScreenDeath()
    {
        float currentY = transform.position.y;

        if (currentY > _maxYPosition || currentY < _minYPosition)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Die();
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
    }
}