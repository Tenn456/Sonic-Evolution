using UnityEngine;

public class RingCounter : MonoBehaviour
{
    public int rings = 0;
    public int redRings = 0;

    public void AddRings(int amount)
    {
        rings += amount;
        Debug.Log("Rings: " + rings);
    }

    public void LoseAllRings()
    {
        rings = 0;
    }

    public void AddRedRings(int amount)
    {
        redRings += amount;
        Debug.Log("Red Rings: " + redRings);
    }
}
