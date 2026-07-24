using UnityEngine;

public class BirdWingAnimator : MonoBehaviour
{
    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    

    [Header("Wing Frames")]
    [Tooltip("Frame 0 = cánh cao, Frame 1 = cánh vừa, Frame 2 = cánh thấp.")]
    [SerializeField] private Sprite _wingUpSprite;

    [SerializeField] private Sprite _wingMidSprite;

    [SerializeField] private Sprite _wingDownSprite;

    [Header("Animation Settings")]
    [Tooltip("Tốc độ vỗ cánh. 8 - 12 là hợp lý.")]
    [SerializeField] private float _framesPerSecond = 10f;

    [Tooltip("Tự chạy vỗ cánh khi game bắt đầu.")]
    [SerializeField] private bool _playOnStart = true;

    private Sprite[] _animationFrames;
    private int _currentFrameIndex;
    private float _timer;
    private bool _isPlaying;

    private void Awake()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
            {
                _spriteRenderer =
                    GetComponentInChildren<SpriteRenderer>();
            }
        }

        BuildAnimationFrames();
    }

    private void Start()
    {
        if (_playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying ||
            Time.timeScale == 0f ||
            _spriteRenderer == null ||
            _animationFrames == null ||
            _animationFrames.Length == 0)
        {
            return;
        }

        _timer += Time.deltaTime;

        float frameDuration =
            1f / _framesPerSecond;

        if (_timer >= frameDuration)
        {
            _timer -= frameDuration;

            _currentFrameIndex++;

            if (_currentFrameIndex >= _animationFrames.Length)
            {
                _currentFrameIndex = 0;
            }

            _spriteRenderer.sprite =
                _animationFrames[_currentFrameIndex];
        }
    }

    private void BuildAnimationFrames()
    {
        /*
         * Thứ tự này giúp chuyển động mượt:
         * Cánh cao → cánh vừa → cánh thấp → cánh vừa → lặp lại.
         */
        _animationFrames = new Sprite[]
        {
            _wingUpSprite,
            _wingMidSprite,
            _wingDownSprite,
            _wingMidSprite
        };
    }

    public void Play()
    {
        BuildAnimationFrames();

        if (_animationFrames == null ||
            _animationFrames.Length == 0 ||
            _wingUpSprite == null ||
            _wingMidSprite == null ||
            _wingDownSprite == null)
        {
            Debug.LogWarning(
                "BirdWingAnimator: Chưa gán đủ 3 sprite cánh cao/cánh vừa/cánh thấp."
            );

            return;
        }

        _isPlaying = true;
        _currentFrameIndex = 0;
        _timer = 0f;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite =
                _animationFrames[_currentFrameIndex];
        }
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void SetWingSprites(
        Sprite wingUp,
        Sprite wingMid,
        Sprite wingDown,
        bool playImmediately = true
    )
    {
        _wingUpSprite = wingUp;
        _wingMidSprite = wingMid;
        _wingDownSprite = wingDown;

        BuildAnimationFrames();

        _currentFrameIndex = 0;
        _timer = 0f;

        if (_spriteRenderer != null &&
            _wingUpSprite != null)
        {
            _spriteRenderer.sprite =
                _wingUpSprite;
        }

        if (playImmediately)
        {
            Play();
        }
    }
}