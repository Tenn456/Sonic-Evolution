using UnityEngine;
using UnityEngine.UI;

public class BoostMeterUI : MonoBehaviour
{
    [Header("References")]
    public Sonic sonic;        // Drag your Sonic object here
    public Image boostFill;    // Drag BoostBarFill Image here

    [Header("Smoothing")]
    public float smooth = 12f; // Higher = snappier UI

    float shown;               // what the UI is currently displaying (0..1)

    void Awake()
    {
        // Start full (or match Sonic immediately)
        shown = 1f;

        // If you want it to start at the actual value:
        if (sonic != null)
            shown = sonic.Boost01;
    }

    void Update()
    {
        if (sonic == null || boostFill == null) return;

        // Target value from Sonic (0..1)
        float target = Mathf.Clamp01(sonic.Boost01);

        // Smoothly move UI toward target so it looks nice
        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // Apply to Image fill
        boostFill.fillAmount = shown;
    }
}
