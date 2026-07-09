using UnityEngine;

public class PipeIncreaseScore : MonoBehaviour
{
    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasTriggered) return;
        if (!collision.CompareTag("Player")) return;

        _hasTriggered = true;

        if (Score.instance != null)
        {
            Score.instance.UpdateScore();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterPipePassed();
        }
    }
}