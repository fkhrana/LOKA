using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class AksaraFragmentItem : MonoBehaviour
{
    [SerializeField] private float fallDistance = 1.5f;
    [SerializeField] private float fallDuration = 0.4f;

    private SpriteRenderer spriteRenderer;
    private AksaraData aksaraData;
    private Coroutine fallCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(AksaraData data, Vector2 spawnPosition)
    {
        if (data == null)
            return;

        aksaraData = data;

        if (spriteRenderer != null)
            spriteRenderer.sprite = data.IconSprite;

        transform.position = spawnPosition;

        if (fallCoroutine != null)
            StopCoroutine(fallCoroutine);
        fallCoroutine = StartCoroutine(FallCoroutine(spawnPosition));
    }

    private IEnumerator FallCoroutine(Vector2 startPos)
    {
        Camera cam = Camera.main;
        float camBottomY = cam.transform.position.y - cam.orthographicSize;
    
        float padding = 0.5f;
        float minY = camBottomY + padding;
    
        Vector2 endPos = startPos + Vector2.down * fallDistance;
        endPos.y = Mathf.Max(endPos.y, minY);
    
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = Mathf.Clamp01(elapsed / fallDuration);
            transform.position = Vector2.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    private void OnMouseDown()
    {
        if (aksaraData != null && CollectedAksaraManager.Instance != null)
        {
            CollectedAksaraManager.Instance.RegisterCollect(aksaraData);
        }
        Destroy(gameObject);
    }
}
