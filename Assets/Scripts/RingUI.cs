using UnityEngine;
using TMPro;

public class RingUI : MonoBehaviour
{
    public RingCounter ringCounter;
    public TextMeshProUGUI ringText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ringText.text = "Rings: " + ringCounter.rings;
    }
}
