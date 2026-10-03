using UnityEngine;

[CreateAssetMenu(fileName = "New Carousel Entry", menuName = "UI/Carousel Entry", order = 0)]
public class CarouselEntry : ScriptableObject
{
    [field:SerializeField] public Sprite EntryGraphic { get; private set; }
    [field:SerializeField] public string Headline { get; private set; }
    [field:SerializeField, Multiline(10)] public string Description { get; private set; }

    [Header("Video (opsional)")]
    [Tooltip("Path relatif dari folder StreamingAssets, contoh: Video/tutorial1.mp4. Kosongkan kalau slide ini hanya gambar.")]
    [field:SerializeField] public string VideoFileName { get; private set; }

    [Header("Special Slide (opsional)")]
    [Tooltip("Kalau true, slide ini tampil sebagai panel khusus (bukan video), dan textbox carousel di-hide.")]
    [field:SerializeField] public bool IsSpecialSlide { get; private set; }

    [field:SerializeField] public string Subtitle { get; private set; }
    [field:SerializeField] public string ButtonText { get; private set; }
}