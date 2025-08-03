using UnityEngine;

public class Fireball_Explode : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the parent GameObject that has the Animator component
        GameObject parent = animator.gameObject;

        // Add collision detection component if not present
        if (parent.GetComponent<Collider2D>() == null)
        {
            parent.AddComponent<Collider2D>();
        }

        // Add collision handling script
        if (parent.GetComponent<FireballCollision>() == null)
        {
            parent.AddComponent<FireballCollision>();
        }
    }
}

public class FireballCollision : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Fireball collided with: {collision.gameObject.name}");
        animator.SetBool("Trigger", true);
    }
}   