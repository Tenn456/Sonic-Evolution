using UnityEngine;

public class Ring : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public int ringValue = 1;

    public AudioClip collectClip;

    void Update()
    {
        // Rotate the ring around its Y axis
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
            ringCounter.AddRings(ringValue);
            audioSource.PlayOneShot(collectClip);
        }

        sonic.GainBoost(5f);

        Destroy(gameObject);
    }
}
