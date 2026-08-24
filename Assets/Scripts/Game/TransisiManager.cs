using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransisiManager : MonoBehaviour
{
    public static TransisiManager Instance;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (fadePanel == null)
        {
            Debug.LogError("TransisiManager: Fade Panel belum di-assign!");
            return;
        }

        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;

        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
            return;

        if (!IsValidScene(sceneName))
        {
            Debug.LogError(
                $"TransisiManager: Scene '{sceneName}' tidak ditemukan!"
            );
            return;
        }

        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(sceneName);

        if (loadOperation == null)
        {
            Debug.LogError(
                $"TransisiManager: Gagal memuat scene '{sceneName}'!"
            );

            isTransitioning = false;
            yield break;
        }

        yield return loadOperation;
        yield return null;

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;

        fadePanel.blocksRaycasts = true;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / fadeDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            fadePanel.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;

        fadePanel.blocksRaycasts = true;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / fadeDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            fadePanel.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private bool IsValidScene(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (name == sceneName)
                return true;
        }

        return false;
    }
}