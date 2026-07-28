using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
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

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        float scale = Mathf.Max(scaleX, scaleY);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}