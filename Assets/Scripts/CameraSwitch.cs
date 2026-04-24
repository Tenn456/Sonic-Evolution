using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Sonic sonic;

    public CinemachineCamera normalCam;
    public CinemachineCamera powerDriftCam;

    public int normalPriority = 10;
    public int driftPriority = 20;

    void LateUpdate()
    {
        if (sonic == null || normalCam == null || powerDriftCam == null)
            return;

        if (sonic.PowerDrifting)
        {
            normalCam.Priority = normalPriority;
            powerDriftCam.Priority = driftPriority;
        }
        else
        {
            normalCam.Priority = driftPriority;
            powerDriftCam.Priority = normalPriority;
        }
    }
}
