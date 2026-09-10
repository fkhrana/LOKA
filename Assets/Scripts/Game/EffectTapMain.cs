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

    private void Start()
    {
        if (tapImage == null)
        {
            Debug.LogWarning("EffectTapMain: tapImage not assigned!");
            return;
        }

        LeanTween.scale(tapImage, Vector3.one * scaleAmount, duration)
            .setLoopPingPong()
            .setEase(LeanTweenType.easeInOutSine);

        float targetY = tapImage.localPosition.y + moveAmount;

        LeanTween.moveLocalY(tapImage.gameObject, targetY, duration)
            .setLoopPingPong()
            .setEase(LeanTweenType.easeInOutSine);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSFXName))
            AudioManager.Instance.PlayHoverSFX(hoverSFXName, hoverSFXVolume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSFXName))
            AudioManager.Instance.PlaySFX(clickSFXName, clickSFXVolume);

        string sceneToLoad = GameProgressManager.HasProgress()
            ? GameProgressManager.GetLastScene(firstLevelScene)
            : firstLevelScene;

        Debug.Log(
            GameProgressManager.HasProgress()
                ? "[EffectTapMain] RESUME GAME → " + sceneToLoad
                : "[EffectTapMain] NEW GAME → " + sceneToLoad
        );

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDestroy()
    {
        if (tapImage != null)
            LeanTween.cancel(tapImage.gameObject);
    }
}