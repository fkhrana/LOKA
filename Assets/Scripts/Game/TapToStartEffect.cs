using UnityEngine;

public class TapToStartEffect : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private RectTransform tapImage;
    [SerializeField] private RectTransform handImage;

    private void Start()
    {
        // Animasi tulisan
        LeanTween.scale(tapImage, Vector3.one * 1.05f, 0.8f)
                 .setLoopPingPong()
                 .setEaseInOutSine();

        LeanTween.moveLocalY(tapImage.gameObject,
                             tapImage.localPosition.y + 8f,
                             0.8f)
                 .setLoopPingPong()
                 .setEaseInOutSine();

        // Animasi tangan
        LeanTween.moveLocalY(handImage.gameObject,
                             handImage.localPosition.y - 10f,
                             0.5f)
                 .setLoopPingPong()
                 .setEaseInOutSine();

        LeanTween.scale(handImage, Vector3.one * 1.08f, 0.5f)
                 .setLoopPingPong()
                 .setEaseInOutSine();
    }
}