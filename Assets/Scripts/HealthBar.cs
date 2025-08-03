using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillBar;
    public float health;

    public void LoseHealth(int value)
    {
        if (health <= 0)
            return;
        health -= value;
        fillBar.fillAmount = health / 100;
    }

    private void Update()
    {
        if (health <= 0)
        {
            FindObjectOfType<Fox>().Die();
        }
    }
    public void SetHealth(int current, int max)
    {
        fillBar.fillAmount = (float)current / max;
    }
}