using UnityEngine;

public class DroppedRing : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public int ringValue = 1;

    public float pickupDelay = 0.75f;

    private bool canBePickedUp;

    public AudioClip collectClip;

    void Start()
    {
        canBePickedUp = false;
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    private void Update()
    {
        // Rotate the ring around its Y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    void EnablePickup()
    {
        canBePickedUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canBePickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        RingCounter ringCounter = other.GetComponent<RingCounter>();
        AudioSource audioSource = other.transform.Find("Sonic Audio").GetComponent<AudioSource>();

        if (ringCounter != null)
        {
            ringCounter.AddRings(ringValue);
            audioSource.PlayOneShot(collectClip);
        }

        Destroy(gameObject);
    }
}
