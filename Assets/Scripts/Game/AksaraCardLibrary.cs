using System.Collections.Generic;
using UnityEngine;

// Asset terpisah, TIDAK mengubah AksaraData.cs.
// Dipakai khusus untuk gambar card di carousel (beda dari FragmentSprite/IconSprite di AksaraData).
[CreateAssetMenu(fileName = "Aksara Card Visual Library", menuName = "LOKA/Aksara Card Visual Library")]
public class AksaraCardVisualLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public AksaraData aksaraData;
        public Sprite cardSprite;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Dictionary<AksaraData, Sprite> lookup;

    private void BuildLookupIfNeeded()
    {
        if (lookup != null) return;

        lookup = new Dictionary<AksaraData, Sprite>();
        foreach (Entry entry in entries)
        {
            if (entry == null || entry.aksaraData == null || entry.cardSprite == null)
                continue;

            if (!lookup.ContainsKey(entry.aksaraData))
                lookup.Add(entry.aksaraData, entry.cardSprite);
        }
    }

    public Sprite GetCardSprite(AksaraData data)
    {
        BuildLookupIfNeeded();

        if (data != null && lookup.TryGetValue(data, out Sprite sprite))
            return sprite;

        return null;
    }
}