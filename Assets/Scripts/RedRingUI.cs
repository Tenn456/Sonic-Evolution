using UnityEngine;
using TMPro;

public class RedRingUI : MonoBehaviour
{
    public RingCounter ringCounter;
    public TextMeshProUGUI ringText;

    // Update is called once per frame
    void Update()
    {
        ringText.text = ringCounter.redRings + " / 5";
    }
}
