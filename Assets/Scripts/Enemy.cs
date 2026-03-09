using UnityEngine;

public class Enemy : MonoBehaviour
{
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
        Sonic sonic = other.GetComponent<Sonic>();

        if (sonic != null)
        {
            sonic.TakeDamage(transform.position);
        }
    }
}
