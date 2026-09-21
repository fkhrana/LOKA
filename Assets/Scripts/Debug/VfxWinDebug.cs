using UnityEngine;
using UnityEngine.UI;

public class VfxWinDebug : MonoBehaviour
{
    private void Awake()
    {
        LogState("Awake");
    }

    private void OnEnable()
    {
        LogState("OnEnable");
    }

    private void Start()
    {
        LogState("Start");
    }

    private void OnDisable()
    {
        LogState("OnDisable");
    }

    private void LogState(string phase)
    {
        var canvas = GetComponentInParent<Canvas>(true);
        var rect = GetComponent<RectTransform>();
        var image = GetComponent<Image>();

        string parentChain = GetParentChain();
        string imageInfo = image != null ? $"imageAlpha={image.color.a} imageEnabled={image.enabled}" : "image=null";
        string rectInfo = rect != null ? $"scale={rect.localScale} size={rect.rect.size} anchored={rect.anchoredPosition}" : "rect=null";
        string canvasInfo = canvas != null ? $"canvasActive={canvas.gameObject.activeSelf} renderMode={canvas.renderMode} sorting={canvas.sortingOrder}" : "canvas=null";

        Debug.Log($"[VfxWinDebug] {phase}: name={name} active={gameObject.activeSelf} parent={transform.parent?.name ?? "null"} {canvasInfo} {imageInfo} {rectInfo} parentChain={parentChain}");

        foreach (Transform child in transform)
        {
            Image childImage = child.GetComponent<Image>();
            if (childImage != null)
            {
                Debug.Log($"[VfxWinDebug] ChildImage: child={child.name} alpha={childImage.color.a} enabled={childImage.enabled} active={child.gameObject.activeSelf} scale={child.localScale}");
            }
        }
    }

    private string GetParentChain()
    {
        Transform current = transform;
        string chain = current.name;

        while (current.parent != null)
        {
            current = current.parent;
            chain += " -> " + current.name;
        }

        return chain;
    }
}
