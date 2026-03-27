using UnityEngine;

public class HomingReticleUI : MonoBehaviour
{
    public Sonic sonic;
    public RectTransform reticle;
    public Camera cam;
    public Vector3 worldOffset = new Vector3(0f, 1f, 0f);

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // Hide at start
        if (reticle != null)
        {
            reticle.gameObject.SetActive(false);
        }
            
    }

    void Update()
    {
        if (sonic == null || reticle == null || cam == null)
            return;

        Transform target = sonic.CurrentHomingTarget;

        if (target == null)
        {
            reticle.gameObject.SetActive(false);
            return;
        }

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Hide if behind camera
        if (screenPos.z <= 0f)
        {
            reticle.gameObject.SetActive(false);
            return;
        }

        // Show reticle
        reticle.gameObject.SetActive(true);
        reticle.position = screenPos;

        if (target == null)
        {
            Debug.Log("No current homing target");
            reticle.gameObject.SetActive(false);
            return;
        }

        //Debug.Log("Current homing target: " + target.name);
        //Debug.Log("Screen position: " + screenPos);
    }
}