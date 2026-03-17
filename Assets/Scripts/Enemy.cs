using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform enemyRootTransform;

    private void OnTriggerEnter(Collider other)
    {
        Sonic sonic = other.GetComponent<Sonic>();

        if (sonic != null)
        {
            sonic.TakeDamage(enemyRootTransform.position);
        }
    }
}
