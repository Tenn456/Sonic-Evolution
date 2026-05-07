using UnityEngine;

public class SonidEndIdle : MonoBehaviour
{
    public Sonic sonic;
    public Animator animator;

    public void EndIdleAnimation()
    {
        sonic.idlePlay = false;
        animator.SetBool("Idle1", false);

        // Reset timer so it doesn't instantly replay
        sonic.idleTimer = 0f;
    }
}
