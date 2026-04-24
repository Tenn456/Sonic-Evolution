using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    public Image fadeImage;
    public float delay = 2f;
    public float fadeDuration = 1f;

    private bool isRunning;

    private void Awake()
    {
        Instance = this;
    }

    public void HandleDeath()
    {
        if (isRunning) return;
        isRunning = true;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(delay);

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = timer / fadeDuration;
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}