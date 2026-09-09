using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EffectTapMain : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [Header("Image")]
    [SerializeField] private RectTransform tapImage;

    [Header("Animation Settings")]
    [SerializeField] private float scaleAmount = 1.05f;
    [SerializeField] private float moveAmount = 8f;
    [SerializeField] private float duration = 0.8f;

    [Header("SFX")]
    [SerializeField] private string hoverSFXName = "Hover";

    [Range(0f, 1f)]
    [SerializeField] private float hoverSFXVolume = 0.1f;

    [SerializeField] private string clickSFXName = "Click";

    [Range(0f, 1f)]
    [SerializeField] private float clickSFXVolume = 1f;

    [Header("Scene Settings")]
    [Tooltip("Scene pertama yang dimainkan jika belum ada progress.")]
    [SerializeField] private string firstLevelScene = "FirstLevelScene";


    // =========================
    // START
    // =========================

    private void Start()
    {
        if (tapImage == null)
        {
            Debug.LogWarning(
                "EffectTapMain: tapImage not assigned!"
            );

            return;
        }

        // Scale animation
        LeanTween.scale(
            tapImage,
            Vector3.one * scaleAmount,
            duration
        )
        .setLoopPingPong()
        .setEase(
            LeanTweenType.easeInOutSine
        );


        // Move animation
        float targetY =
            tapImage.localPosition.y + moveAmount;

        LeanTween.moveLocalY(
            tapImage.gameObject,
            targetY,
            duration
        )
        .setLoopPingPong()
        .setEase(
            LeanTweenType.easeInOutSine
        );
    }


    // =========================
    // HOVER
    // =========================

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (AudioManager.Instance != null &&
            !string.IsNullOrEmpty(hoverSFXName))
        {
            AudioManager.Instance.PlayHoverSFX(
                hoverSFXName,
                hoverSFXVolume
            );
        }
    }


    // =========================
    // CLICK
    // =========================

    public void OnPointerClick(
        PointerEventData eventData
    )
    {
        // Play click SFX
        if (AudioManager.Instance != null &&
            !string.IsNullOrEmpty(clickSFXName))
        {
            AudioManager.Instance.PlaySFX(
                clickSFXName,
                clickSFXVolume
            );
        }


        string sceneToLoad;


        // =========================
        // RESUME
        // =========================

        if (GameProgressManager.HasProgress())
        {
            sceneToLoad =
                GameProgressManager.GetLastScene(
                    firstLevelScene
                );

            Debug.Log(
                "[EffectTapMain] RESUME GAME → "
                + sceneToLoad
            );
        }


        // =========================
        // NEW GAME
        // =========================

        else
        {
            sceneToLoad = firstLevelScene;

            Debug.Log(
                "[EffectTapMain] NEW GAME → "
                + sceneToLoad
            );
        }


        // Load scene
        SceneManager.LoadScene(
            sceneToLoad
        );
    }


    // =========================
    // DESTROY
    // =========================

    private void OnDestroy()
    {
        if (tapImage != null)
        {
            LeanTween.cancel(
                tapImage.gameObject
            );
        }
    }
}