using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Sonic sonic;

    public CinemachineCamera normalCam;
    public CinemachineCamera powerDriftCam;

    public int activePriority = 20;
    public int inactivePriority = 10;

    void Awake()
    {
        SetNormalCam();
    }

    void LateUpdate()
    {
        if (sonic == null || normalCam == null || powerDriftCam == null)
            return;

        if (sonic.PowerDrifting)
            SetDriftCam();
        else
            SetNormalCam();
    }

    void SetNormalCam()
    {
        normalCam.Priority = activePriority;
        powerDriftCam.Priority = inactivePriority;
    }

    void SetDriftCam()
    {
        normalCam.Priority = inactivePriority;
        powerDriftCam.Priority = activePriority;
    }
}
