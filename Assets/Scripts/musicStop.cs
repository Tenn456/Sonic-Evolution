using UnityEngine;

public class musicStop : MonoBehaviour
{
    public AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!audioSource.mute)
        {
            audioSource.mute = true;
        }
        else
        {
            audioSource.mute = false;
        }
    }
}
