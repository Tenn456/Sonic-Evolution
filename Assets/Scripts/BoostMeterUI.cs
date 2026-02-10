using UnityEngine;
using UnityEngine.UI;

public class BoostMeterUI : MonoBehaviour
{
    [Header("References")]
    public Sonic sonic;
    public Image boostFill;

    [Header("Smoothing")]
    public float smooth = 12f; // Higher = snappier UI

    float shown;

    void Awake()
    {
        // Start full
        shown = 1f;
    }

    void Update()
    {
        if (sonic == null || boostFill == null) return;

        // Target value from Sonic (0..1)
        float target = Mathf.Clamp01(sonic.Boost01);

        // Smoothly move UI toward target
        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // Apply to Image fill
        boostFill.fillAmount = shown;
    }
}
