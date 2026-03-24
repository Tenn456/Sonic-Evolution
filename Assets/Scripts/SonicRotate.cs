using UnityEngine;

public class SonicRotate : MonoBehaviour
{
    // For Homing Attack Animation (It is facing wrong way)

    public void RotateSonic()
    {
        transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }

    public void UnRotateSonic()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
