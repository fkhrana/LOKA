using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CurtainsAnimation : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float closeDuration = 0.5f;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float openDuration = 0.5f;

    [Header("Sprite (opsional, jika pakai Image)")]
    [SerializeField] private Sprite defaultSprite; // sprite awal (terbuka)

    private static CurtainsAnimation instance;
    private Image image;

    public static void TransitionToScene(string sceneName)
    {
        if (instance != null)
        {
            Debug.LogWarning("[Curtains] Transisi sudah berjalan.");
            return;
        }

        // 🔥 Ganti nama prefab sesuai dengan yang ada di Resources
        GameObject prefab = Resources.Load<GameObject>("Curtains");
        if (prefab == null)
        {
            Debug.LogError("[Curtains] Prefab 'CurtainsPrefab' tidak ditemukan di Resources! Coba nama lain.");
            // Fallback: coba "Curtains"
            prefab = Resources.Load<GameObject>("Curtains");
            if (prefab == null)
            {
                Debug.LogError("[Curtains] Prefab 'Curtains' juga tidak ditemukan! Load scene langsung.");
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        GameObject obj = Instantiate(prefab);
        DontDestroyOnLoad(obj);

        instance = obj.GetComponent<CurtainsAnimation>();
        if (instance == null)
        {
            Debug.LogError("[Curtains] Komponen CurtainsAnimation tidak ditemukan di prefab!");
            Destroy(obj);
            SceneManager.LoadScene(sceneName);
            return;
        }

        instance.StartTransition(sceneName);
    }

 private void Awake()
{
    // Cari Image di GameObject yang sama, atau di parent, atau di child
    image = GetComponent<Image>();
    if (image == null)
        image = GetComponentInParent<Image>();
    if (image == null)
        image = GetComponentInChildren<Image>();

    if (image == null)
    {
        Debug.LogError("[Curtains] Tidak ada komponen Image di mana pun dalam hierarchy!");
        return;
    }

    if (animator == null)
        animator = GetComponent<Animator>();
    if (animator == null)
        animator = GetComponentInParent<Animator>();
    if (animator == null)
        animator = GetComponentInChildren<Animator>();

    if (defaultSprite != null)
        image.sprite = defaultSprite;
}
    private void StartTransition(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // 1. Tutup curtain
        if (animator != null)
            animator.SetTrigger("startClose");
        else
            Debug.LogWarning("[Curtains] Animator null, tidak bisa tutup.");

        yield return new WaitForSecondsRealtime(closeDuration);

        // 2. Jeda
        yield return new WaitForSecondsRealtime(holdDuration);

        // 3. Load scene
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
            yield return null;
        async.allowSceneActivation = true;
        yield return null;

        // 4. Buka curtain
        if (animator != null)
            animator.SetTrigger("startOpen");
        else
            Debug.LogWarning("[Curtains] Animator null, tidak bisa buka.");

        yield return new WaitForSecondsRealtime(openDuration);

        // 5. Selesai, hancurkan
        instance = null;
        Destroy(gameObject);
    }
}