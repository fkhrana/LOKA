using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    [SerializeField, Min(0f)] private float extraCoverage = 0.5f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        FitToScreen();
    }

    void FitToScreen()
    {
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        float scaleX = (cameraWidth + extraCoverage * 2f) / spriteWidth;
        float scaleY = (cameraHeight + extraCoverage * 2f) / spriteHeight;

        float scale = Mathf.Max(scaleX, scaleY);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}