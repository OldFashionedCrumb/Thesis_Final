using UnityEngine;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    // Reference to existing GameObjects with SpriteRenderers
    [SerializeField] private GameObject backgroundBar;
    [SerializeField] private GameObject healthFillBar;

    // Serialized field to control health percentage from the Inspector
    [SerializeField] [Range(0, 100)] private float healthPercentage = 100f; // Changed default to 100f

    // Original scale of the health fill bar
    private Vector3 originalScale;

    // Original position of the health fill bar
    private Vector3 originalPosition;

    private void Start()
    {
        // Always initialize at full health
        currentHealth = maxHealth;
        healthPercentage = 100f;

        // Verify the GameObjects exist
        if (backgroundBar == null || healthFillBar == null)
        {
            Debug.LogError("Health bar GameObjects not assigned!");
            return;
        }

        // Store original scale and position
        originalScale = healthFillBar.transform.localScale;
        originalPosition = healthFillBar.transform.localPosition;

        // Initialize health bar at full health
        UpdateHealthBar(100f);
    }

    // Rest of the code remains unchanged
    private void Update()
    {
        // Debug.Log(healthPercentage);
        // Check if the health percentage was changed in the inspector during runtime
        float calculatedPercentage = (currentHealth / maxHealth) * 100f;
        if (!Mathf.Approximately(calculatedPercentage, healthPercentage))
        {
            currentHealth = maxHealth * (healthPercentage / 100f);
            UpdateHealthBar(healthPercentage);
        }
    }

    // Update health bar visuals based on percentage (0-100)
    public void UpdateHealthBar(float newPercentage)
    {
        // Make sure the percentage is between 0 and 100
        newPercentage = Mathf.Clamp(newPercentage, 0f, 100f);

        // Update the serialized field
        healthPercentage = newPercentage;

        // Calculate the scale factor (0.0 to 1.0)
        float scaleFactor = healthPercentage / 100f;

        // Update the scale of the fill bar on x-axis only
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * scaleFactor;
        healthFillBar.transform.localScale = newScale;

        // Adjust position to ensure scaling happens from left to right
        Vector3 newPosition = originalPosition;
        float offset = (originalScale.x - newScale.x) * 0.5f;

        // Dynamic position adjustment formula based on health percentage:
        // -25 at 0%, -10 at 50%, 0 at 100%
        float dynamicAdjustment = 0.25f * healthPercentage - 25f;

        newPosition.x = originalPosition.x - offset + dynamicAdjustment;
        healthFillBar.transform.localPosition = newPosition;
    }

    // Method to take damage and update the health bar
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        healthPercentage = (currentHealth / maxHealth) * 100f;
        UpdateHealthBar(healthPercentage);
    }

    // Method to heal and update the health bar
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);

        healthPercentage = (currentHealth / maxHealth) * 100f;
        UpdateHealthBar(healthPercentage);
    }
}