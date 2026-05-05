using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public GameObject enemyRoot;
    public AudioSource audioSource;
    public AudioClip deathClip;

    public void TakeHit()
    {
        audioSource.PlayOneShot(deathClip);
        Destroy(enemyRoot);
    }
}
