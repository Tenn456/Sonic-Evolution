using UnityEngine;

public class spinnerRotate : MonoBehaviour
{
    public float rotationSpeed = 100f; // Degrees per second

    // Update is called once per frame
    void Update()
    {
        // Rotate z axis
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
