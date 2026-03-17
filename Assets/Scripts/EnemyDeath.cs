using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public GameObject enemyRoot;

    public void TakeHit()
    {
        Destroy(enemyRoot);
    }
}
