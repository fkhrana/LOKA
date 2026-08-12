using UnityEngine;

public class TapToStartEffect : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private RectTransform tapImage;

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
    }
}