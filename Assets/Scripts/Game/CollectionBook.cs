using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject collectionPanel;

    private bool isOpen = false;

    public void ToggleCollection()
    {
        if (isOpen)
            CloseCollection();
        else
            OpenCollection();
    }

    public void OpenCollection()
    {
        if (isOpen) return;
        isOpen = true;
        collectionPanel?.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("[Collection] Dibuka");
    }

    public void CloseCollection()
    {
        if (!isOpen) return;
        isOpen = false;

        // Cek apakah collectionPanel punya EffectPanel
        var effect = collectionPanel?.GetComponent<EffectPanel>();
        if (effect != null)
        {
            // Tutup dengan efek
            effect.CloseDialog(() =>
            {
                collectionPanel?.SetActive(false);
                Time.timeScale = 1f;
                Debug.Log("[Collection] Ditutup dengan efek");
            });
        }
        else
        {
            // Langsung ilang tanpa efek (fallback)
            collectionPanel?.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("[Collection] Ditutup langsung (tidak ada EffectPanel)");
        }
    }
}