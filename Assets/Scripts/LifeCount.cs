using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeCount : MonoBehaviour
{
    public Image[] lives;
    public int livesRemaining;

    //4 lives - 4 imgaes (0,1,2,3)
    //3 lives - 3 images (0,1,2,[3])
    //2 lives - 2 images (0,1,[2],[3])
    //1 life - 1 image (0,[1],[2],[3])
    //0 lives - 0 images ([0,1,2,3]) LOSE

   public void LoseLife()
    {
        // If no lives remaining do nothing
        if (livesRemaining == 0)
            return;
        // Decrease the value of livesRemaining
        livesRemaining--;
        // Hide one of the life images
        lives[livesRemaining].enabled = false;
    
        // Get the Fox instance
        Fox fox = FindObjectOfType<Fox>();
    
        // If we run out of lives, trigger hurt and die
        if (livesRemaining == 0)
        {
            if (fox != null)
                fox.TakeDamage(fox.maxHealth); // Ensure death and hurt animation
        }
        else
        {
            if (fox != null)
                fox.TakeDamage(1); // Trigger hurt animation
        }
    }
    
    public void AddLife()
    {
        // Only add life if we don't have maximum lives
        if (livesRemaining >= lives.Length)
            return;
    
        // Enable the life image
        lives[livesRemaining].enabled = true;
    
        // Increase the lives remaining
        livesRemaining++;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LoseLife();
        if (livesRemaining == 0)
        {
            LevelManager lm = FindObjectOfType<LevelManager>();
            if (lm != null)
                lm.Restart();
        }
    }

}
