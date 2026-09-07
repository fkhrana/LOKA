using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class AksaraFragmentItem : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float sideDistance = -1.5f;
    [SerializeField] private float dropDuration = 0.4f;
    [SerializeField] private GameObject dropVfx;

    private SpriteRenderer spriteRenderer;
    private AksaraData aksaraData;
    private Coroutine fallCoroutine;
    private ParticleSystem[] dropVfxParticles;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (dropVfx != null)
        {
            dropVfxParticles = dropVfx.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particles in dropVfxParticles)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Initialize(AksaraData data, Vector2 spawnPosition)
    {
        if (data == null)
            return;

        aksaraData = data;

        if (spriteRenderer != null)
            spriteRenderer.sprite = data.IconSprite;

        transform.position = spawnPosition;

        if (dropVfx != null)
            dropVfx.SetActive(true);

        if (dropVfxParticles != null)
        {
            foreach (ParticleSystem particles in dropVfxParticles)
                particles.Play(true);
        }

        if (fallCoroutine != null)
            StopCoroutine(fallCoroutine);
        fallCoroutine = StartCoroutine(DropCoroutine(spawnPosition));
    }

    private IEnumerator DropCoroutine(Vector2 startPos)
    {
        Camera cam = Camera.main;
        float minY = startPos.y - 1.5f;

        if (cam != null)
        {
            float camBottomY = cam.transform.position.y - cam.orthographicSize;
            minY = camBottomY + 0.5f;
        }

        Vector2 firstLanding = startPos + new Vector2(sideDistance, -1.5f);
        firstLanding.y = Mathf.Max(firstLanding.y, minY);

        yield return StartCoroutine(JumpCoroutine(startPos, firstLanding, jumpHeight, dropDuration));

        Vector2 finalLanding = firstLanding + new Vector2(sideDistance * 0.2f, 0f);
        yield return StartCoroutine(JumpCoroutine(
            firstLanding,
            finalLanding,
            jumpHeight * 0.3f,
            dropDuration * 0.5f));
    }

    private IEnumerator JumpCoroutine(Vector2 from, Vector2 to, float height, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float arc = Mathf.Sin(progress * Mathf.PI) * height;
            transform.position = Vector2.Lerp(from, to, progress) + Vector2.up * arc;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
    }

    private void OnMouseDown()
    {
        if (aksaraData != null && CollectedAksaraManager.Instance != null)
        {
            CollectedAksaraManager.Instance.RegisterCollect(aksaraData);
            PermanentCollectionManager.SaveCollected(aksaraData);
        }
        Destroy(gameObject);
    }
}
