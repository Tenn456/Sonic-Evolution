using UnityEngine;

public class HomingAttackCheck : StateMachineBehaviour
{
    // For Homing Attack Animation (It is facing wrong way)
    // Checks when Sonic enters and exit animation state

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SonicRotate fix = animator.GetComponent<SonicRotate>();
        if (fix != null)
        {
            fix.RotateSonic();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SonicRotate fix = animator.GetComponent<SonicRotate>();
        if (fix != null)
        {
            fix.UnRotateSonic();
        }
    }
}
