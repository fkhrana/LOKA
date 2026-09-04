using UnityEngine;

public class BannerWiggle : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform banner;

    [Header("Animation")]
    [SerializeField] private float angle = 5f;
    [SerializeField] private float duration = 0.5f;

    private Quaternion originalRotation;

    private void Awake()
    {
        if (banner == null)
            banner = GetComponent<RectTransform>();

        originalRotation = banner.localRotation;
    }

    private void Start()
    {
        Wiggle();
    }

    public void Wiggle()
    {
        if (banner == null)
            return;

        LeanTween.cancel(banner.gameObject);

        banner.localRotation = originalRotation;

        LeanTween.rotateZ(
            banner.gameObject,
            angle,
            duration
        )
        .setEaseInOutSine()
        .setLoopPingPong();
    }

    private void OnDisable()
    {
        if (banner != null)
        {
            LeanTween.cancel(banner.gameObject);
            banner.localRotation = originalRotation;
        }
    }
}