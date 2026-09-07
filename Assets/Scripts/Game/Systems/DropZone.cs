using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone")]
    public string zoneTag;

    [HideInInspector]
    public bool isFilled = false;

    [Header("SFX")]
    [SerializeField] private AudioClip correctSFX;
    [SerializeField] private AudioClip wrongSFX;

    [Header("VFX")]
    [SerializeField] private GameObject correctVFX;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 15f;

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
            originalAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DragItem draggedItem =
            eventData.pointerDrag.GetComponent<DragItem>();

        if (draggedItem == null) return;

        // ===== BENAR =====
        if (draggedItem.correctTargetTag == zoneTag)
        {
            if (isFilled)
            {
                draggedItem.ReturnToStart();
                return;
            }

            draggedItem.transform.SetParent(transform);
            draggedItem.transform.localPosition = Vector3.zero;

            isFilled = true;

            Image img = draggedItem.GetComponent<Image>();

            if (img != null)
                img.raycastTarget = false;

            // SFX BENAR
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(correctSFX);

            // VFX BENAR
            PlayCorrectEffect();

            // CEK PUZZLE
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.CheckPuzzleComplete();

            Debug.Log("✅ Benar! Item masuk ke " + zoneTag);
        }

        // ===== SALAH =====
        else
        {
            draggedItem.ReturnToStart();

            // SFX SALAH
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(wrongSFX);

            // SHAKE
            PlayWrongEffect();

            Debug.Log("❌ Salah, kembali ke posisi awal");
        }
    }

    // =========================================================
    // VFX BENAR
    // =========================================================

    private void PlayCorrectEffect()
    {
        if (correctVFX == null)
        {
            Debug.LogWarning(
                "⚠️ Correct VFX belum di-assign pada "
                + gameObject.name
            );

            return;
        }

        GameObject vfx = Instantiate(
            correctVFX,
            transform.position,
            Quaternion.identity
        );

        ParticleSystem[] particles =
            vfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.Play(true);
        }

        Destroy(vfx, 2f);

        Debug.Log(
            "✨ VFX dimainkan pada " + gameObject.name
        );
    }

    // =========================================================
    // EFFECT SALAH
    // =========================================================

    private void PlayWrongEffect()
    {
        if (rectTransform == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakePanel());
    }

    private IEnumerator ShakePanel()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX =
                Random.Range(-1f, 1f) * shakeMagnitude;

            rectTransform.anchoredPosition =
                originalAnchoredPos +
                new Vector2(offsetX, 0f);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition =
            originalAnchoredPos;

        shakeRoutine = null;
    }
}