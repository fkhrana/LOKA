using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    [Header("Collection Panels")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private GameObject collectionPanel2;

    private bool isOpen = false;

    // =========================
    // COLLECTION 1
    // =========================

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

        Debug.Log("[Collection 1] Dibuka");
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

                Debug.Log("[Collection 1] Ditutup dengan efek");
            });
        }
        else
        {
            // Langsung ilang tanpa efek (fallback)
            collectionPanel?.SetActive(false);
            Time.timeScale = 1f;

            Debug.Log("[Collection 1] Ditutup langsung (tidak ada EffectPanel)");
        }
    }


    // =========================
    // COLLECTION 2
    // =========================

    public void ToggleCollection2()
    {
        if (collectionPanel2 == null) return;

        if (collectionPanel2.activeSelf)
            CloseCollection2();
        else
            OpenCollection2();
    }

    public void OpenCollection2()
    {
        if (collectionPanel2 == null) return;

        collectionPanel2.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("[Collection 2] Dibuka");
    }

    public void CloseCollection2()
    {
        if (collectionPanel2 == null) return;

        // Cek apakah collectionPanel2 punya EffectPanel
        var effect = collectionPanel2.GetComponent<EffectPanel>();

        if (effect != null)
        {
            // Tutup dengan efek
            effect.CloseDialog(() =>
            {
                collectionPanel2.SetActive(false);
                Time.timeScale = 1f;

                Debug.Log("[Collection 2] Ditutup dengan efek");
            });
        }
        else
        {
            // Langsung ilang tanpa efek (fallback)
            collectionPanel2.SetActive(false);
            Time.timeScale = 1f;

            Debug.Log("[Collection 2] Ditutup langsung (tidak ada EffectPanel)");
        }
    }
}