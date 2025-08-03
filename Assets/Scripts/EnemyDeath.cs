using UnityEngine;

public class EnemyDeath : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the parent GameObject
        GameObject enemy = animator.gameObject;
        
        // Disable the EnemyAI component to stop movement
        if (enemy.GetComponent<EnemyAI>() != null)
        {
            enemy.GetComponent<EnemyAI>().enabled = false;
        }
        
        // Destroy the enemy after animation completes
        Destroy(enemy, stateInfo.length);
    }
}