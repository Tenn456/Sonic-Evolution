using UnityEngine;

public class RedRing : MonoBehaviour
{
    public float rotationSpeed = 180f; // Degrees per second
    public int ringValue = 1;

    public AudioClip collectClip;

    // Update is called once per frame
    void Update()
    {
        // Rotate y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Sonic sonic = other.GetComponent<Sonic>();

        if (sonic.dead)
            return;

        RingCounter ringCounter = other.GetComponent<RingCounter>();
        AudioSource audioSource = other.transform.Find("Sonic Audio").GetComponent<AudioSource>();

        if (ringCounter != null)
        {
            ringCounter.AddRedRings(ringValue);
            audioSource.PlayOneShot(collectClip);
        }

        Destroy(gameObject);
    }
}
