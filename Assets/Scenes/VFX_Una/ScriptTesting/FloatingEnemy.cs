using UnityEngine;

public class FloatingEnemy : MonoBehaviour
{
    public float amplitude = 0.25f; // Seberapa tinggi/rendah naiknya
    public float speed = 2f;        // Seberapa cepat naik-turunnya
    private float startY;

    void Start()
    {
        // Simpan posisi Y awal musuh
        startY = transform.position.y;
    }

    void Update()
    {
        // Gunakan fungsi Sinus untuk membuat gerak naik turun yang mulus
        float newY = startY + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}