using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fox : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    public Collider2D standingCollider, crouchingCollider;
    public Transform groundCheckCollider;
    public Transform overheadCheckCollider;
    public LayerMask groundLayer;
    public Transform wallCheckCollider;
    public LayerMask wallLayer;

    const float groundCheckRadius = 0.2f;
    const float overheadCheckRadius = 0.2f;
    const float wallCheckRadius = 0.2f;
    [SerializeField] float speed = 2;
    [SerializeField] float jumpPower = 500;
    [SerializeField] float slideFactor = 0.2f;
    [SerializeField] private AudioClip jumpSound; // Jump sound effect
    private AudioSource audioSource; // AudioSource component

    public int totalJumps;
    int availableJumps;
    float horizontalValue;
    float runSpeedModifier = 2f;
    float crouchSpeedModifier = 0.5f;
    
    bool isGrounded = true;
    bool isRunning;
    bool facingRight = true;
    bool crouchPressed;
    bool multipleJump;
    bool coyoteJump;
    bool isSliding;
    bool isDead = false;

    // Health system fields
    public int maxHealth = 3;
    private int currentHealth;
    public int GetCurrentHealth() => currentHealth;
    private bool isInvulnerable = false;
    private HealthBar healthBar;

    void Awake()
    {
        availableJumps = totalJumps;
        currentHealth = maxHealth;
        healthBar = FindObjectOfType<HealthBar>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Debug check for LevelManager
        LevelManager lm = FindObjectOfType<LevelManager>();
        if (lm != null)
            Debug.Log("LevelManager found and working properly.");
        else
            Debug.LogError("LevelManager not found in the scene!");
    }

    void Update()
    {
        if (!CanMoveOrInteract())
            return;

        // Store the horizontal value
        horizontalValue = Input.GetAxisRaw("Horizontal");

        // Enable running while LeftShift is held down
        if (Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            isRunning = false;
        
        
        // // Kill player when T is pressed
        // if (Input.GetKeyDown(KeyCode.T))
        //     Die();

        // Check for Jump input
        if (Input.GetButtonDown("Jump"))
            Jump();

        // Check for Crouch input (crouching only allowed when grounded)
        if (Input.GetButtonDown("Crouch") && isGrounded)
            crouchPressed = true;
        else if (Input.GetButtonUp("Crouch"))
            crouchPressed = false;

        // Update vertical velocity parameter for the animator
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        // Check if we are touching a wall to slide on it
        WallCheck();
    }

    void FixedUpdate()
    {
        GroundCheck();
        Move(horizontalValue, crouchPressed);
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(groundCheckCollider.position, groundCheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(overheadCheckCollider.position, overheadCheckRadius);
    }

    bool CanMoveOrInteract()
    {
        bool can = true;
        if (FindObjectOfType<InteractionSystem>().isExamining)
            can = false;
        if (FindObjectOfType<InventorySystem>().isOpen)
            can = false;
        if (isDead)
            can = false;
        return can;
    }

    void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        // Check if the GroundCheckObject is colliding with any colliders in the "Ground" layer.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer);
        if (colliders.Length > 0)
        {
            isGrounded = true;
            if (!wasGrounded)
            {
                availableJumps = totalJumps;
                multipleJump = false;
            }
            // If colliding with a moving platform, parent the fox to the platform.
            foreach (var c in colliders)
            {
                if (c.tag == "MovingPlatform")
                    transform.parent = c.transform;
            }
        }
        else
        {
            transform.parent = null;
            if (wasGrounded)
                StartCoroutine(CoyoteJumpDelay());
        }
        animator.SetBool("Jump", !isGrounded);
    }

    void WallCheck()
    {
        if (Physics2D.OverlapCircle(wallCheckCollider.position, wallCheckRadius, wallLayer)
            && Mathf.Abs(horizontalValue) > 0
            && rb.linearVelocity.y < 0
            && !isGrounded)
        {
            if (!isSliding)
            {
                availableJumps = totalJumps;
                multipleJump = false;
            }
            Vector2 v = rb.linearVelocity;
            v.y = -slideFactor;
            rb.linearVelocity = v;
            isSliding = true;

            if (Input.GetButtonDown("Jump"))
            {
                availableJumps--;
                rb.linearVelocity = Vector2.up * jumpPower;
                animator.SetBool("Jump", true);
            }
        }
        else
        {
            isSliding = false;
        }
    }

    #region Jump
    IEnumerator CoyoteJumpDelay()
    {
        coyoteJump = true;
        yield return new WaitForSeconds(0.2f);
        coyoteJump = false;
    }

    void Jump()
    {
        if (isGrounded)
        {
            multipleJump = true;
            availableJumps--;
            rb.linearVelocity = Vector2.up * jumpPower;
            animator.SetBool("Jump", true);
            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
        else
        {
            if (coyoteJump)
            {
                multipleJump = true;
                availableJumps--;
                rb.linearVelocity = Vector2.up * jumpPower;
                animator.SetBool("Jump", true);
                if (jumpSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(jumpSound);
                }
            }

            if (multipleJump && availableJumps > 0)
            {
                availableJumps--;
                rb.linearVelocity = Vector2.up * jumpPower;
                animator.SetBool("Jump", true);
                if (jumpSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(jumpSound);
                }
            }
        }
    }
    #endregion

    void Move(float dir, bool crouchFlag)
    {
        #region Crouch
        // When mid-air, disable crouching.
        if (!isGrounded)
            crouchFlag = false;
        else
        {
            // Check overhead for collision with ground items – if any, remain crouched.
            if (!crouchFlag)
            {
                if (Physics2D.OverlapCircle(overheadCheckCollider.position, overheadCheckRadius, groundLayer))
                    crouchFlag = true;
            }
        }

        animator.SetBool("Crouch", crouchFlag);
        standingCollider.enabled = !crouchFlag;
        crouchingCollider.enabled = crouchFlag;
        #endregion

        #region Move & Run
        float xVal = dir * speed * 100 * Time.fixedDeltaTime;
        // Multiply with the running modifier if running.
        if (isRunning)
            xVal *= runSpeedModifier;
        // Multiply with the crouching modifier if crouched.
        if (crouchFlag)
            xVal *= crouchSpeedModifier;
        Vector2 targetVelocity = new Vector2(xVal, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;

        if (facingRight && dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = false;
        }
        else if (!facingRight && dir > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = true;
        }
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        #endregion
    }

    public void Die()
    {
        isDead = true;
        FindObjectOfType<LevelManager>().Restart();
    }

    public void ResetPlayer()
    {
        isDead = false;
        currentHealth = maxHealth;
    }

    // New method to apply damage to the fox and trigger the hurt animation.
    
    public void TakeDamage(int damage)
    {
        // Skip damage if player is dead or currently invulnerable
        if (isDead || isInvulnerable)
            return;

        currentHealth -= damage;
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth > 0)
        {
            // Trigger the hurt animation
            animator.SetTrigger("Hurt");
        
            // Start coroutine to reset the Hurt trigger after animation completes
            StartCoroutine(ResetHurtTrigger());

            // Start invulnerability period
            StartCoroutine(InvulnerabilityPeriod());
        }
        else
        {
            // Optionally play the hurt animation just before dying
            animator.SetTrigger("Hurt");
            Die();
        }
    }

    private IEnumerator ResetHurtTrigger()
    {
        // Wait for the hurt animation to complete
        // Get the current state info from the Animator's base layer (0)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    
        // Reset the Hurt trigger
        animator.ResetTrigger("Hurt");
    }

    private IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;
        
        // Optional visual feedback for invulnerability
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float blinkInterval = 0.15f;
        
        // Blink for 3 seconds
        for (float i = 0; i < 3; i += blinkInterval)
        {
            if (spriteRenderer)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // Ensure sprite is visible when finished
        if (spriteRenderer)
            spriteRenderer.enabled = true;
        
        isInvulnerable = false;
    }
    public void SetCurrentHealth(int value)
    {
        currentHealth = value;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);
    }
}