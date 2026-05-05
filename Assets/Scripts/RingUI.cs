using UnityEngine;
using TMPro;

public class RingUI : MonoBehaviour
{
    public RingCounter ringCounter;
    public TextMeshProUGUI ringText;

    // Update is called once per frame
    void Update()
    {
        ringText.text = ringCounter.rings.ToString();
    }
}
