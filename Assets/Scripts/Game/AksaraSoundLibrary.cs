using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Aksara Sound Library",
    menuName = "LOKA/Aksara Sound Library"
)]
public class AksaraSoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public GestureShape gestureShape;
        public AudioClip clip;

        [Tooltip("Volume 0..1. Nilai >1 tidak berefek karena PlayOneShot clamp ke 1. Boost loudness via AudioMixerGroup 'AksaraVoice'.")]
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<GestureShape, Entry> lookup;

    // Build lookup sekali saja saat pertama dipakai.
    private void BuildLookupIfNeeded()
    {
        if (lookup != null) return;

        lookup = new Dictionary<GestureShape, Entry>();

        foreach (Entry entry in entries)
        {
            if (entry == null || entry.clip == null) continue;
            if (!lookup.ContainsKey(entry.gestureShape)) lookup.Add(entry.gestureShape, entry);
        }
    }

    // Ambil clip untuk shape; null kalau tidak ada.
    public AudioClip GetClip(GestureShape shape)
    {
        BuildLookupIfNeeded();

        if (lookup.TryGetValue(shape, out Entry entry)) return entry.clip;

        return null;
    }

    // Ambil volume untuk shape; fallback 1.
    public float GetVolume(GestureShape shape)
    {
        BuildLookupIfNeeded();

        if (lookup.TryGetValue(shape, out Entry entry)) return Mathf.Clamp01(entry.volume);

        return 1f;
    }
}