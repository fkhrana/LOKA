using System.Collections;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthHeartVFX : MonoBehaviour
{
    [SerializeField] private Image heartImage;
    [SerializeField] private GameObject lostHpVfxObject;

    private UIParticle lostHpVfx;

    private Coroutine damageAnimation;

    private void Awake()
    {
        if (heartImage == null)
            heartImage = GetComponent<Image>();

        if (lostHpVfxObject != null)
        {
            lostHpVfx = lostHpVfxObject.GetComponentInChildren<UIParticle>(true);
            lostHpVfxObject.SetActive(false);
        }
    }

    public void PlayDamage()
    {
        if (damageAnimation != null)
            StopCoroutine(damageAnimation);

        damageAnimation = StartCoroutine(PlayDamageRoutine());
    }

    public void ResetVisual()
    {
        if (damageAnimation != null)
        {
            StopCoroutine(damageAnimation);
            damageAnimation = null;
        }

        if (lostHpVfx != null)
            lostHpVfx.Stop();

        if (lostHpVfxObject != null)
            lostHpVfxObject.SetActive(false);

        transform.localScale = Vector3.one;

        if (heartImage != null)
        {
            heartImage.enabled = true;
            heartImage.color = Color.white;
        }
    }

    private IEnumerator PlayDamageRoutine()
    {
        float elapsed = 0f;

        while (elapsed < 0.3f)
        {
            if (heartImage != null)
                heartImage.color = Mathf.PingPong(elapsed * 20f, 1f) > 0.5f ? Color.white : Color.red;

            float shakeScale = Mathf.PingPong(elapsed * 15f, 0.3f) + 0.85f;
            transform.localScale = new Vector3(shakeScale, shakeScale, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        if (heartImage != null)
            heartImage.color = Color.white;

        while (elapsed < 0.15f)
        {
            float swellScale = Mathf.Lerp(1.1f, 1.5f, elapsed / 0.15f);
            transform.localScale = new Vector3(swellScale, swellScale, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (heartImage != null)
            heartImage.enabled = false;

        if (lostHpVfxObject != null && !lostHpVfxObject.activeSelf)
            lostHpVfxObject.SetActive(true);

        if (lostHpVfx != null)
            lostHpVfx.Play();

        damageAnimation = null;
    }
}