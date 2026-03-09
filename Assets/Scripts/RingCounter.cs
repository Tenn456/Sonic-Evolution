using UnityEngine;

public class RingCounter : MonoBehaviour
{
    public int rings = 0;

    public void AddRings(int amount)
    {
        rings += amount;
        Debug.Log("Rings: " + rings);
    }

    public void LoseAllRings()
    {
        rings = 0;
    }
}
