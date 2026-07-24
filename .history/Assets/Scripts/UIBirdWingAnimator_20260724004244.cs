using UnityEngine;
using UnityEngine.UI;

public class UIBirdWingAnimator : MonoBehaviour
{
    [Header("UI Image")]
    [SerializeField] private Image _birdImage;

    [Header("Wing Frames")]
    [SerializeField] private Sprite _wingUpSprite;
    [SerializeField] private Sprite _wingMidSprite;
    [SerializeField] private Sprite _wingDownSprite;

    [Header("Animation")]
    [SerializeField] private float _framesPerSecond = 8f;
    [SerializeField] private bool _playOnStart = false;

    private Sprite[] _frames;
    private int _currentFrameIndex;
    private float _timer;
    private bool _isPlaying;

    private void Awake()
    {
        if (_birdImage == null)
        {
            _birdImage = GetComponent<Image>();
        }

        BuildFrames();
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
            _birdImage == null ||
            _frames == null ||
            _frames.Length == 0)
        {
            return;
        }

        _timer += Time.unscaledDeltaTime;

        float frameDuration =
            1f / _framesPerSecond;

        if (_timer >= frameDuration)
        {
            _timer -= frameDuration;

            _currentFrameIndex++;

            if (_currentFrameIndex >= _frames.Length)
            {
                _currentFrameIndex = 0;
            }

            _birdImage.sprite =
                _frames[_currentFrameIndex];
        }
    }

    private void BuildFrames()
    {
        _frames = new Sprite[]
        {
            _wingUpSprite,
            _wingMidSprite,
            _wingDownSprite,
            _wingMidSprite
        };
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

        BuildFrames();

        _currentFrameIndex = 0;
        _timer = 0f;

        if (_birdImage != null &&
            _wingMidSprite != null)
        {
            _birdImage.sprite =
                _wingMidSprite;
        }

        if (playImmediately)
        {
            Play();
        }
    }

    public void Play()
    {
        if (_birdImage == null ||
            _wingUpSprite == null ||
            _wingMidSprite == null ||
            _wingDownSprite == null)
        {
            return;
        }

        _isPlaying = true;
        _currentFrameIndex = 0;
        _timer = 0f;

        _birdImage.sprite =
            _wingUpSprite;
    }

    public void Stop()
    {
        _isPlaying = false;

        if (_birdImage != null &&
            _wingMidSprite != null)
        {
            _birdImage.sprite =
                _wingMidSprite;
        }
    }
}