using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject enemyRoot;
    public Transform enemyRootTransform;

    public AudioClip deathClip;

    private void OnTriggerEnter(Collider other)
    {
        Sonic sonic = other.GetComponent<Sonic>();

        if (sonic != null)
        {
            if (sonic.jumping || sonic.dropDashCharging)
            {
                AudioSource audioSource = other.transform.Find("Sonic Audio").GetComponent<AudioSource>();

                // Give a small upward force (bounce)
                sonic.velocity.y = sonic.jumpForce;
                audioSource.PlayOneShot(deathClip);
                sonic.GainBoost(10f);
                Destroyed();
            }
            else if (sonic.spindashRolling || sonic.boosting || sonic.stomping || sonic.spindashCharging)
            {
                AudioSource audioSource = other.transform.Find("Sonic Audio").GetComponent<AudioSource>();

                audioSource.PlayOneShot(deathClip);
                sonic.GainBoost(10f);
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
