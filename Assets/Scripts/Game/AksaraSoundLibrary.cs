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

        [Range(0f, 500f)]
        public float volume = 500f;
    }

    [SerializeField]
    private List<Entry> entries =
        new List<Entry>();

    private Dictionary<GestureShape, Entry> lookup;

    private void BuildLookupIfNeeded()
    {
        if (lookup != null)
            return;

        lookup =
            new Dictionary<GestureShape, Entry>();

        foreach (Entry entry in entries)
        {
            if (
                entry == null ||
                entry.clip == null
            )
                continue;

            if (!lookup.ContainsKey(entry.gestureShape))
            {
                lookup.Add(
                    entry.gestureShape,
                    entry
                );
            }
        }
    }

    public AudioClip GetClip(
        GestureShape shape
    )
    {
        BuildLookupIfNeeded();

        if (
            lookup.TryGetValue(
                shape,
                out Entry entry
            )
        )
        {
            return entry.clip;
        }

        return null;
    }

    public float GetVolume(
        GestureShape shape
    )
    {
        BuildLookupIfNeeded();

        if (
            lookup.TryGetValue(
                shape,
                out Entry entry
            )
        )
        {
            return Mathf.Clamp(
                entry.volume,
                0f,
                500f
            );
        }

        return 500f;
    }
}