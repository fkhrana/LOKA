using System.Collections.Generic;
using UnityEngine;

// Asset terpisah, TIDAK mengubah AksaraData.cs.
// Dipakai untuk mencari AudioClip pelafalan berdasarkan GestureShape milik AksaraData.
[CreateAssetMenu(fileName = "Aksara Sound Library", menuName = "LOKA/Aksara Sound Library")]
public class AksaraSoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public GestureShape gestureShape;
        public AudioClip clip;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Dictionary<GestureShape, AudioClip> lookup;

    private void BuildLookupIfNeeded()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<GestureShape, AudioClip>();
        foreach (Entry entry in entries)
        {
            if (entry == null || entry.clip == null)
                continue;

            if (!lookup.ContainsKey(entry.gestureShape))
                lookup.Add(entry.gestureShape, entry.clip);
        }
    }

    public AudioClip GetClip(GestureShape shape)
    {
        BuildLookupIfNeeded();

        if (lookup.TryGetValue(shape, out AudioClip clip))
            return clip;

        return null;
    }
}
