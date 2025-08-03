using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyDeathAnimationController : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;
    [SerializeField] private int baseHealth = 1; // Base health that will be modified
    private int health;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on " + gameObject.name);
        }

        string currentScene = SceneManager.GetActiveScene().name;
        SetEnemyHealth(currentScene);
    }

    private void SetEnemyHealth(string currentScene)
    {
        // Default health setup based on scene
        if (currentScene == "Scene1")
        {
            health = baseHealth; // Scene1 always starts with base health (1)
        }
        else
        {
            health = 2; // Other scenes start with 2 health
        }

        // Check previous scene completion time
        string previousScene = GetPreviousSceneName(currentScene);
        if (!string.IsNullOrEmpty(previousScene))
        {
            float previousSceneTime = SceneTimeManager.GetSceneTime(previousScene);
            if (previousSceneTime > 0 && previousSceneTime < 30f)
            {
                health += 1; // Increase health if previous level was completed quickly
                Debug.Log($"Enemy health increased by 1 due to fast completion of {previousScene} ({previousSceneTime:F2}s)");
            }
        }

        Debug.Log($"Enemy health set to {health} in {currentScene}");
    }

    private string GetPreviousSceneName(string currentScene)
    {
        // Extract scene number if it exists
        if (int.TryParse(currentScene.Replace("Scene", ""), out int currentSceneNumber))
        {
            if (currentSceneNumber > 1)
            {
                return $"Scene{currentSceneNumber - 1}";
            }
        }
        return string.Empty;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("PlayerShootingItem"))
        {
            if (health > 0)
            {
                health--;
                Debug.Log("Enemy hit! Remaining health: " + health);
            }
            if (health == 0 && !isDead) // Only trigger death when health is exactly 0
            {
                isDead = true;
                if (animator != null)
                {
                    animator.SetTrigger("Death");
                }
                Collider2D myCollider = GetComponent<Collider2D>();
                if (myCollider != null)
                {
                    myCollider.enabled = false;
                }
                var ai = GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.enabled = false;
                }
                StartCoroutine(DisableAfterDelay(0f));
            }
        }
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}

