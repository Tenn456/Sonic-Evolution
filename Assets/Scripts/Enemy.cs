using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject enemyRoot;
    public Transform enemyRootTransform;

    private void OnTriggerEnter(Collider other)
    {
        Sonic sonic = other.GetComponent<Sonic>();

        if (sonic != null)
        {
            if (sonic.jumping)
            {
                // Give a small upward force (bounce)
                sonic.velocity.y = sonic.jumpForce;
                Destroyed();
            }
            else if (sonic.spindashRolling || sonic.boosting || sonic.stomping)
            {
                Destroyed();
            }
            else
            {
                sonic.TakeDamage(enemyRootTransform.position);
            }

        }
    }

    public void Destroyed()
    {
        Destroy(enemyRoot);
    }
}
