using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    private enum PanelType { None, Pause, Collection }

    [Header("Panels")]
    [SerializeField] private GameObject pauseContainer;
    [SerializeField] private GameObject collectionPanel;

    private PanelType currentPanel = PanelType.None;

    // ---------- PAUSE ----------

    public void OpenPause()
    {
        if (currentPanel == PanelType.Pause)
            return;

        CloseAllPanels();

        pauseContainer?.SetActive(true);
        currentPanel = PanelType.Pause;

        PauseOverlay pause = pauseContainer?.GetComponent<PauseOverlay>();

        if (pause != null)
            pause.OpenPause();
        else
            Time.timeScale = 0f;
    }

    public void ClosePause()
    {
        if (currentPanel != PanelType.Pause)
            return;

        PauseOverlay pause = pauseContainer?.GetComponent<PauseOverlay>();

        if (pause != null)
            pause.ClosePause();
        else
        {
            pauseContainer?.SetActive(false);
            Time.timeScale = 1f;
        }

        currentPanel = PanelType.None;
    }

    public void TogglePause()
    {
        if (currentPanel == PanelType.Pause)
            ClosePause();
        else
            OpenPause();
    }

    // ---------- COLLECTION ----------

    public void OpenCollection()
    {
        if (currentPanel == PanelType.Collection)
            return;

        CloseAllPanels();

        collectionPanel?.SetActive(true);
        currentPanel = PanelType.Collection;

        Time.timeScale = 0f;
    }

    public void CloseCollection()
    {
        if (currentPanel != PanelType.Collection)
            return;

        EffectPanel effect = collectionPanel?.GetComponent<EffectPanel>();

        if (effect != null)
        {
            effect.CloseDialog(() =>
            {
                collectionPanel?.SetActive(false);
                Time.timeScale = 1f;
            });
        }
        else
        {
            collectionPanel?.SetActive(false);
            Time.timeScale = 1f;
        }

        currentPanel = PanelType.None;
    }

    public void ToggleCollection()
    {
        if (currentPanel == PanelType.Collection)
            CloseCollection();
        else
            OpenCollection();
    }

    // ---------- CLOSE ALL ----------

    public void CloseAllPanels()
    {
        pauseContainer?.SetActive(false);
        collectionPanel?.SetActive(false);

        currentPanel = PanelType.None;
    }
}