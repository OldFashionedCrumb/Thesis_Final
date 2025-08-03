using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class TrapObject : MonoBehaviour
{
    // Configurable parameters
    [Header("Trap Timing")]
    [SerializeField] private float defaultVisibleDuration = 1f;
    [SerializeField] private float fastVisibleDuration = 0.2f;
    [SerializeField] private float defaultRevealDelay = 1f;
    [SerializeField] private float fastRevealDelay = 0.5f;
    [SerializeField] private float timeThreshold = 30f; // Time threshold for increasing difficulty

    private Coroutine damageCoroutine;
    private Coroutine loopCoroutine;
    private BoxCollider2D boxCollider;
    private SpriteRenderer trapSprite;
    private SpriteRenderer[] childSprites;
    private float revealDelay;
    private float visibleDuration;
    private bool isRevealed = false;
    [SerializeField]private bool isVisible = false;
    private bool isScene3;
    private bool isSlamReveal = false;

    private void Start()
    {
        Initialize();
        AdjustTrapDifficulty();
        isScene3 = SceneManager.GetActiveScene().name == "Scene3";
    }

    private void Initialize()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        childSprites = GetComponentsInChildren<SpriteRenderer>(true);
        
        // Set default values
        visibleDuration = defaultVisibleDuration;
        revealDelay = defaultRevealDelay;

        if (CompareTag("HiddenTrap"))
        {
            foreach (var sprite in childSprites)
            {
                if (sprite != null)
                    sprite.enabled = false;
            }
            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
            Debug.Log($"Hidden trap with {childSprites.Length} spikes initialized");
        }
    }

    private void AdjustTrapDifficulty()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string previousScene = GetPreviousSceneName(currentSceneName);
        
        if (!string.IsNullOrEmpty(previousScene))
        {
            float previousSceneTime = SceneTimeManager.GetSceneTime(previousScene);
            if (previousSceneTime > 0 && previousSceneTime < timeThreshold)
            {
                // Increase difficulty
                visibleDuration = fastVisibleDuration;
                revealDelay = fastRevealDelay;
                Debug.Log($"Trap difficulty increased! Previous scene ({previousScene}) completed in {previousSceneTime:F2} seconds");
            }
        }
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

    private void Reset()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Trap] OnTriggerEnter2D: Collided with {collision.gameObject.name}, Tag: {collision.tag}");

        // In Scene3, allow HiddenTrap to deal damage if visible
        if (isScene3 && CompareTag("HiddenTrap"))
        {
            if (collision.CompareTag("Player") && isVisible)
            {
                if (damageCoroutine == null)
                {
                    Debug.Log($"[Trap] Start DelayedDamage for {collision.gameObject.name}");
                    damageCoroutine = StartCoroutine(DelayedDamage(collision));
                }
                else
                {
                    Debug.Log($"[Trap] DelayedDamage already running for {collision.gameObject.name}");
                }
            }
            return;
        }

        if (collision.CompareTag("Player"))
        {
            if (CompareTag("HiddenTrap") && !isRevealed)
            {
                loopCoroutine = StartCoroutine(HiddenTrapAppearDisappearLoop(collision));
            }
            else if (!CompareTag("HiddenTrap"))
            {
                // Normal trap: show spikes and start delayed damage
                foreach (var sprite in childSprites)
                    if (sprite != null) sprite.enabled = true;
                isVisible = true; // Ensure normal traps are marked visible
                damageCoroutine = StartCoroutine(DelayedDamage(collision));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // In Scene3, stop damage when player leaves HiddenTrap
        if (isScene3 && CompareTag("HiddenTrap"))
        {
            if (collision.CompareTag("Player"))
            {
                if (damageCoroutine != null)
                {
                    Debug.Log($"[Trap] Stop DamageOverTime for {collision.gameObject.name}");
                    StopCoroutine(damageCoroutine);
                    damageCoroutine = null;
                }
            }
            return;
        }

        if (collision.CompareTag("Player"))
        {
            if (loopCoroutine != null)
                StopCoroutine(loopCoroutine);
            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);
            // Hide all child sprites for HiddenTrap only
            if (CompareTag("HiddenTrap"))
            {
                foreach (var sprite in childSprites)
                    if (sprite != null) sprite.enabled = false;
                isRevealed = false;
            }
            // For normal traps, do NOT hide spikes on exit
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check for player staying inside trap
        if (collision.CompareTag("Player"))
        {
            // For HiddenTrap in Scene3, only damage if visible
            if (isScene3 && CompareTag("HiddenTrap"))
            {
                if (isVisible && damageCoroutine == null)
                {
                    if (isSlamReveal) {
                        Debug.Log($"[Trap] OnTriggerStay2D: Player {collision.gameObject.name} is inside and trap is visible (slam reveal). Doing immediate damage.");
                        LifeCount life = FindObjectOfType<LifeCount>();
                        if (life != null) life.LoseLife();
                        damageCoroutine = StartCoroutine(DamageOverTime(collision));
                        isSlamReveal = false;
                    } else {
                        Debug.Log($"[Trap] OnTriggerStay2D: Player {collision.gameObject.name} is inside and trap is visible. Starting DelayedDamage.");
                        damageCoroutine = StartCoroutine(DelayedDamage(collision));
                    }
                }
                return;
            }
            // For normal traps or revealed hidden traps
            if ((!CompareTag("HiddenTrap") || (CompareTag("HiddenTrap") && isRevealed)) && isVisible)
            {
                if (damageCoroutine == null)
                {
                    Debug.Log($"[Trap] OnTriggerStay2D: Player {collision.gameObject.name} is inside trap. Starting DelayedDamage.");
                    damageCoroutine = StartCoroutine(DelayedDamage(collision));
                }
            }
        }
    }

    public void RevealForSlam()
    {
        if (CompareTag("HiddenTrap"))
        {
            // Stop any existing routines
            if (loopCoroutine != null)
                StopCoroutine(loopCoroutine);
            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);
            isSlamReveal = true;
            StartCoroutine(SlamRevealRoutine());
        }
    }

    private IEnumerator SlamRevealRoutine()
    {
        foreach (var sprite in childSprites)
            if (sprite != null) sprite.enabled = true;
        isVisible = true;

        // Check for any collider on the trap and see if it's the player
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                // Do damage immediately
                LifeCount life = FindObjectOfType<LifeCount>();
                if (life != null)
                {
                    life.LoseLife();
                }
                // Start damage over time as well
                damageCoroutine = StartCoroutine(DamageOverTime(col));
                break;
            }
        }

        yield return new WaitForSeconds(visibleDuration);

        foreach (var sprite in childSprites)
            if (sprite != null) sprite.enabled = false;
        isVisible = false;
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
        isSlamReveal = false;
    }

    IEnumerator HiddenTrapAppearDisappearLoop(Collider2D player)
    {
        isRevealed = true;
        yield return new WaitForSeconds(revealDelay);
        while (true)
        {
            // Appear
            foreach (var sprite in childSprites)
                if (sprite != null) sprite.enabled = true;
            isVisible = true; // Ensure isVisible is set for hidden trap
            
            // Do immediate damage when trap appears
            LifeCount life = FindObjectOfType<LifeCount>();
            if (life != null)
            {
                Debug.Log($"[Trap] HiddenTrap: Player {player.gameObject.name} is losing life immediately on trap appear.");
                life.LoseLife();
            }
            
            // Then start damage over time
            damageCoroutine = StartCoroutine(DamageOverTime(player));
            
            yield return new WaitForSeconds(visibleDuration);
            // Disappear
            foreach (var sprite in childSprites)
                if (sprite != null) sprite.enabled = false;
            isVisible = false; // Reset isVisible when hidden
            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);
            yield return new WaitForSeconds(revealDelay);
        }
    }

    IEnumerator DamageOverTime(Collider2D player)
    {
        LifeCount life = FindObjectOfType<LifeCount>();
        if (life == null)
        {
            Debug.LogWarning("No LifeCount component found in the scene!");
            yield break;
        }
        yield return new WaitForSeconds(3f); // Wait before first damage tick
        while (true)
        {
            Debug.Log($"[Trap] DamageOverTime: Player {player.gameObject.name} is losing life.");
            life.LoseLife();
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator DelayedDamage(Collider2D player)
    {
        if (isVisible && player != null)
        {
            LifeCount life = FindObjectOfType<LifeCount>();
            if (life != null)
            {
                Debug.Log($"[Trap] DelayedDamage: Player {player.gameObject.name} is losing life immediately.");
                life.LoseLife();
            }
            damageCoroutine = StartCoroutine(DamageOverTime(player));
        }
        yield break; // This was missing!
    }
}
