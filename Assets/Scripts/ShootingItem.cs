using UnityEngine;
using System.Collections;

public class ShootingItem : MonoBehaviour
{
 public float speed;
    private Animator animator;
    private bool hasExploded = false;
    private Rigidbody2D rb;
    public Transform spriteTransform; // Assign in Inspector if needed

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (spriteTransform == null)
            spriteTransform = transform; // fallback to self
    }
private void Update()
{
    if (!hasExploded)
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;

            // Adjust for flipped scale (facing left)
            if (spriteTransform.localScale.x < 0)
                angle += 180f;

            spriteTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}

 private void OnTriggerEnter2D(Collider2D collision)
{
    // Ignore collisions with player or other shooting items
    if (collision.tag == "Player" || collision.tag == "Item" || collision.name.StartsWith("Cherry") || collision.GetComponent<ShootingItem>())
        return;

    // Handle enemy collision
    if (collision.CompareTag("Enemy"))
    {
        // Always trigger explosion, but let the enemy handle its own health/death logic
        var deathController = collision.GetComponent<EnemyDeathAnimationController>();
        if (deathController != null)
        {
            // The enemy will decrement health and handle death/animation
            // No need to check health here
        }
    }
    // Handle boss (GruzMother) collision
    if (collision.CompareTag("GruzMother"))
    {
        GruzMother boss = collision.GetComponent<GruzMother>();
        if (boss != null)
        {
            boss.TakeDamage(10f); // 1 fireball = 10 HP
            Debug.Log("Boss hit! -10 HP");
        }
    }

    hasExploded = true;
    if (rb != null)
        rb.linearVelocity = Vector2.zero; // Stop movement immediately

    animator.SetBool("Explode", true);
    StartCoroutine(WaitForExplosionToEnd());
}

    private IEnumerator WaitForExplosionToEnd()
    {
        // Wait for the explosion animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }       
}