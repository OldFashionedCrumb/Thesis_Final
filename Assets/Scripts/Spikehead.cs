using UnityEngine;

public class Spikehead : MonoBehaviour
{
    [Header("SpikeHead Attributes")]
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private float checkDelay;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Vector3[] directions = new Vector3[4];
    private Vector3 destination;
    private float checkTimer;
    private bool attacking;
    private Transform playerTransform;
    private bool playerDetected;

    private LifeCount lifeCount;
    private Animator animator;

    private void OnEnable()
    {
        Stop();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Find the player and LifeCount at start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
            
        lifeCount = FindObjectOfType<LifeCount>();
        if (lifeCount == null)
            Debug.LogWarning("No LifeCount component found!");

        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (attacking)
        {
            if (playerTransform != null && playerDetected)
            {
                // Update destination to continuously chase player
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                destination = directionToPlayer;
                transform.Translate(destination * Time.deltaTime * speed);
            }
            else
            {
                transform.Translate(destination * Time.deltaTime * speed);
            }
        }
        else
        {
            checkTimer += Time.deltaTime;
            if (checkTimer > checkDelay)
                CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        CalculateDirections();

        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawRay(transform.position, directions[i], Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], range, playerLayer);

            if (hit.collider != null && !attacking)
            {
                attacking = true;
                playerDetected = true;
                destination = directions[i];
                checkTimer = 0;
            }
        }
    }

    private void CalculateDirections()
    {
        directions[0] = transform.right * range;
        directions[1] = -transform.right * range;
        directions[2] = transform.up * range;
        directions[3] = -transform.up * range;
    }

    private void Stop()
    {
        destination = transform.position;
        attacking = false;
        playerDetected = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && lifeCount != null)
        {
            lifeCount.LoseLife();
        }
        else if (collision.CompareTag("PlayerShootingItem"))
        {
            TakeDamage(1); // Take 1 damage from projectiles
            Destroy(collision.gameObject); // Destroy the projectile
        }
        Stop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && lifeCount != null)
        {
            lifeCount.LoseLife();
        }
        else if (collision.gameObject.CompareTag("PlayerShootingItem"))
        {
            TakeDamage(1); // Take 1 damage from projectiles
            Destroy(collision.gameObject); // Destroy the projectile
        }
        Stop();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Take Damage");
        currentHealth -= damage;
        Debug.Log($"Spikehead took {damage} damage, current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            // Stop all behavior
            Stop();
            attacking = false;
            playerDetected = false;
            
            // Disable all components that could interact with player
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Rigidbody2D>().simulated = false;
            enabled = false; // Disable this script
            
            // Make sure the object can't move or deal damage
            transform.parent = null;
            destination = transform.position;
            
            // Destroy the gameObject after a short delay
            Destroy(gameObject, 0.5f);
        }
        else
        {
            // Optional: Add hit feedback like blinking
            StartCoroutine(BlinkEffect());
        }
    }

    private System.Collections.IEnumerator BlinkEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
}
