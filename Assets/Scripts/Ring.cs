using UnityEngine;

public class Ring : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public int ringValue = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        // Rotate the ring around its Y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        RingCounter ringCounter = other.GetComponent<RingCounter>();
        if (ringCounter != null)
        {
            ringCounter.AddRings(ringValue);
        }

        Destroy(gameObject);
    }
}
