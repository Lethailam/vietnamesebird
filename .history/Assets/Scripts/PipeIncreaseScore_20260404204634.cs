using UnityEngine;

public class PipeIncreaseScore : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Score.instance.UpdateScore();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RegisterPipePassed();
            }
        }
    }
}