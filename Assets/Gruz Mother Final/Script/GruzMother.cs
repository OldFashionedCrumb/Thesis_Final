using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GruzMother : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private BossHealthBar healthBar;
    private bool isEnraged = false;

    [Header("Speed Multipliers")]
    [SerializeField] private float enragedSpeedMultiplier = 1.5f; // Speed multiplier when below 50% health

    [Header("Idel")]
    [SerializeField] float idelMovementSpeed;
    [SerializeField] Vector2 idelMovementDirection;
    private float currentIdelSpeed;

    [Header("AttackUpNDown")]
    [SerializeField] float attackMovementSpeed;
    [SerializeField] Vector2 attackMovementDirection;
    private float currentAttackSpeed;

    [Header("AttackPlayer")]
    [SerializeField] float attackPlayerSpeed;
    [SerializeField] GameObject player;
    private float currentPlayerAttackSpeed;

    [Header("Other")]
    [SerializeField] Transform goundCheckUp;
    [SerializeField] Transform goundCheckDown;
    [SerializeField] Transform goundCheckWall;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask groundLayer;

    private bool isTouchingUp;
    private bool isTouchingDown;
    private bool isTouchingWall;
    private bool hasPlayerPositon;

    private Vector2 playerPosition;

    private bool facingLeft = true;
    private bool goingUp = true;
    private Rigidbody2D enemyRB;
    private Animator enemyAnim;

    void Start()
    {
        idelMovementDirection.Normalize();
        attackMovementDirection.Normalize();
        enemyRB = GetComponent<Rigidbody2D>();
        enemyAnim = GetComponent<Animator>();

        // Initialize health and speeds
        currentHealth = maxHealth;
        currentIdelSpeed = idelMovementSpeed;
        currentAttackSpeed = attackMovementSpeed;
        currentPlayerAttackSpeed = attackPlayerSpeed;

        // Find health bar if not assigned
        if (healthBar == null)
        {
            healthBar = FindObjectOfType<BossHealthBar>();
            if (healthBar == null)
                Debug.LogError("BossHealthBar not found in the scene!");
        }
    }

    void Update()
    {
        isTouchingUp = Physics2D.OverlapCircle(goundCheckUp.position, groundCheckRadius, groundLayer);
        isTouchingDown = Physics2D.OverlapCircle(goundCheckDown.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(goundCheckWall.position, groundCheckRadius, groundLayer);
    }

    void RandomStatePicker()
    {
        int randomState = Random.Range(0, 4); // Changed from 0,2 to 0,4 to increase slam chance
        if (randomState <= 2) // 75% chance (3 out of 4) for slam attack
        {
            enemyAnim.SetTrigger("AttackPlayer"); // This leads to slam animation when hitting wall/ground
        }
        else
        {
            enemyAnim.SetTrigger("AttackUpNDown");
        }
    }

    public void IdelState()
    {
        if (isTouchingUp && goingUp)
        {
            ChangeDirection();
        }
        else if (isTouchingDown && !goingUp)
        {
            ChangeDirection();
        }

        if (isTouchingWall)
        {
            if (facingLeft)
            {
                Flip();
            }
            else if (!facingLeft)
            {
                Flip();
            }
        }
        enemyRB.linearVelocity = currentIdelSpeed * idelMovementDirection;
    }

    public void AttackUpNDownState()
    {
        if (isTouchingUp && goingUp)
        {
            ChangeDirection();
        }
        else if (isTouchingDown && !goingUp)
        {
            ChangeDirection();
        }

        if (isTouchingWall)
        {
            if (facingLeft)
            {
                Flip();
            }
            else if (!facingLeft)
            {
                Flip();
            }
        }
        enemyRB.linearVelocity = currentAttackSpeed * attackMovementDirection;
    }

    public void AttackPlayerState()
    {
        if (!hasPlayerPositon)
        {
            FlipTowardsPlayer();
            playerPosition = player.transform.position - transform.position;
            playerPosition.Normalize();
            hasPlayerPositon = true;
        }
        if (hasPlayerPositon)
        {
            enemyRB.linearVelocity = currentPlayerAttackSpeed * playerPosition;
        }

        if (isTouchingWall || isTouchingDown)
        {
            //play Slam animation
            enemyAnim.SetTrigger("Slamed");
            enemyRB.linearVelocity = Vector2.zero;
            hasPlayerPositon = false;

            // Find and trigger all hidden traps when boss slams
            GameObject[] hiddenTraps = GameObject.FindGameObjectsWithTag("HiddenTrap");
            foreach (GameObject trap in hiddenTraps)
            {
                TrapObject trapObject = trap.GetComponent<TrapObject>();
                if (trapObject != null)
                {
                    trapObject.RevealForSlam();
                }
            }
        }
    }

    void FlipTowardsPlayer()
    {
        float playerDirection = player.transform.position.x - transform.position.x;

        if (playerDirection > 0 && facingLeft)
        {
            Flip();
        }
        else if (playerDirection < 0 && !facingLeft)
        {
            Flip();
        }
    }

    void ChangeDirection()
    {
        goingUp = !goingUp;
        idelMovementDirection.y *= -1;
        attackMovementDirection.y *= -1;
    }

    void Flip()
    {
        facingLeft = !facingLeft;
        idelMovementDirection.x *= -1;
        attackMovementDirection.x *= -1;
        transform.Rotate(0, 180, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("Hit: " + collision.name);
        if (collision.CompareTag("Player"))
        {
            LifeCount lifeCount = FindObjectOfType<LifeCount>();
            if (lifeCount != null)
            {
                lifeCount.LoseLife();
            }
            else
            {
                // Fallback: still damage the player if LifeCount is missing
                Fox playerFox = collision.GetComponent<Fox>() ?? collision.GetComponentInParent<Fox>();
                if (playerFox != null)
                {
                    playerFox.TakeDamage(1);
                    Debug.Log("Hit player and called TakeDamage on: " + playerFox.name);
                }
                else
                {
                    Debug.LogWarning("No Fox script found on collided player object or its parent.");
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Check if health drops below 50% and boss isn't already enraged
        if (currentHealth <= maxHealth / 2 && !isEnraged)
        {
            EnterEnragedState();
        }

        // Update the health bar
        if (healthBar != null)
        {
            healthBar.TakeDamage(damage);
            Debug.Log("Gruz Mother took damage: " + damage + ", Health: " + currentHealth);
        }

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void EnterEnragedState()
    {
        isEnraged = true;
        // Increase all movement speeds
        currentIdelSpeed = idelMovementSpeed * enragedSpeedMultiplier;
        currentAttackSpeed = attackMovementSpeed * enragedSpeedMultiplier;
        currentPlayerAttackSpeed = attackPlayerSpeed * enragedSpeedMultiplier;

        Debug.Log("Boss enraged! Speed increased!");

        // Optional: Play animation or particle effect
        if (enemyAnim != null)
        {
            enemyAnim.SetTrigger("Enraged");
        }
    }

    private void Die()
    {
        // Play death animation if available
        if (enemyAnim != null)
            enemyAnim.SetTrigger("Death");

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
            collider.enabled = false;
        // Hide the boss health bar
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
        // Disable the game object
        // gameObject.SetActive(false);

        StartCoroutine(LoadEndSceneWithDelay());
    }

    private IEnumerator LoadEndSceneWithDelay()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(2f); // Short delay to ensure disappearance is visible

        SceneManager.LoadScene("End");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(goundCheckUp.position, groundCheckRadius);
        Gizmos.DrawWireSphere(goundCheckDown.position, groundCheckRadius);
        Gizmos.DrawWireSphere(goundCheckWall.position, groundCheckRadius);
    }
}
