using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LatihanAksaraItem : MonoBehaviour
{
    [SerializeField] private GestureShape gestureShape = GestureShape.Unknown;
    [SerializeField] private Button voiceOverButton;
    [SerializeField] private LatihanTulisController controller;

    public GestureShape GestureShape
    {
        get { return gestureShape; }
    }

    private void Awake()
    {
        if (voiceOverButton == null)
            voiceOverButton = GetComponentInChildren<Button>(true);

        if (controller == null)
            controller = GetComponentInParent<LatihanTulisController>();

        if (voiceOverButton != null)
            voiceOverButton.onClick.AddListener(PlayVoiceOver);
        else
            Debug.LogWarning("LatihanAksaraItem tidak memiliki Button Voice Over.", this);
    }

    public void PlayVoiceOver()
    {
        if (controller == null)
        {
            Debug.LogWarning(
                "LatihanAksaraItem tidak memiliki LatihanTulisController.",
                this);
            return;
        }

        controller.PlayVoiceOver(gestureShape);
    }

    private void OnDestroy()
    {
        if (voiceOverButton != null)
            voiceOverButton.onClick.RemoveListener(PlayVoiceOver);
    }
}
