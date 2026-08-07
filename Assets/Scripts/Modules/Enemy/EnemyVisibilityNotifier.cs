using UnityEngine;

/// <summary>
/// Notifies LevelProgressManager when this enemy becomes visible to the main camera.
/// Uses a simple viewport check each frame and notifies only once.
/// Attach to instantiated enemy GameObject at spawn time.
/// </summary>
public class EnemyVisibilityNotifier : MonoBehaviour
{
    [Tooltip("Allow slight tolerance outside the viewport (0..1). Useful if you consider partially visible as visible.")]
    [SerializeField] private float viewportMargin = 0.05f;

    private bool hasNotified = false;
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
            mainCam = Camera.current;
    }

    private void Update()
    {
        if (hasNotified)
            return;

        if (mainCam == null)
            return;

        Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
        bool visible = vp.z > 0f && vp.x >= -viewportMargin && vp.x <= 1f + viewportMargin && vp.y >= -viewportMargin && vp.y <= 1f + viewportMargin;

        if (visible)
        {
            hasNotified = true;
            LevelProgressManager.Instance?.OnEnemySpawned();
            // no longer need to check
            Destroy(this);
        }
    }
}
