using UnityEngine;

public class BackgroundFollowCamera : MonoBehaviour
{
    private Camera mainCamera;
    private float offsetX;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera != null)
            offsetX = transform.position.x - mainCamera.transform.position.x;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Vector3 position = transform.position;
        position.x = mainCamera.transform.position.x + offsetX;
        transform.position = position;
    }
}