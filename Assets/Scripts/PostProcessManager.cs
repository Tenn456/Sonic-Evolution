using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour
{
    public PostProcessVolume normalVolume;
    public PostProcessVolume boostVolume;

    public float transitionSpeed = 5f;

    private float targetNormal = 1f;
    private float targetBoost = 0f;

    void Update()
    {
        normalVolume.weight = Mathf.Lerp(normalVolume.weight, targetNormal, Time.deltaTime * transitionSpeed);
        boostVolume.weight = Mathf.Lerp(boostVolume.weight, targetBoost, Time.deltaTime * transitionSpeed);
    }

    public void Normal()
    {
        targetNormal = 1f;
        targetBoost = 0f;
    }

    public void Boost()
    {
        targetNormal = 1f;
        targetBoost = 1f;
    }
}
