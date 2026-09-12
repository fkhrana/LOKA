using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    private enum PanelType { None, Setting, Collection, Level, Credits, Tutorial }

    [Serializable]
    private class PanelData
    {
        public PanelType type;
        public GameObject panel;
        public GameObject button;
    }

    [Header("Panels")]
    [SerializeField] private PanelData[] panels;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSound;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "CutScenee";

    [Header("Transition")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;

    [Header("Main Menu Content")]
    [SerializeField] private GameObject mainMenuContent;
    [SerializeField] private string entryStateName = "";

    private PanelType currentPanel = PanelType.None;
    private bool isTransitioning = false;
    private bool isPanelAnimating = false;

    private void Awake()
    {
        Time.timeScale = 1f;
        CloseAllPanels();
    }

    private void Start()
    {
        CleanupAllTransitions();
        ResetAllCanvases();
        ForceShowMainMenu();
        CloseAllPanels();
        PlayEntryAnimation();

        if (PlayerPrefs.GetInt("OpenTutorial", 0) == 1)
        {
            OpenTutorial();
            PlayerPrefs.SetInt("OpenTutorial", 0);
        }

        StartCoroutine(EnsureMainMenuVisible());
    }

    private PanelData GetPanelData(PanelType type)
    {
        foreach (var p in panels)
            if (p.type == type) return p;
        return null;
    }

    private void OpenPanel(PanelType type)
    {
        if (currentPanel == type || isPanelAnimating) return;

        PlayClickSFX();

        if (currentPanel != PanelType.None)
        {
            isPanelAnimating = true;
            ClosePanel(currentPanel, () =>
            {
                isPanelAnimating = false;
                OpenPanelDirect(type);
            });
        }
        else
        {
            OpenPanelDirect(type);
        }
    }

    private void OpenPanelDirect(PanelType type)
    {
        var data = GetPanelData(type);
        if (data == null) return;

        data.panel?.SetActive(true);
        data.button?.SetActive(false);
        currentPanel = type;
    }

    private void ClosePanel(PanelType type, Action onComplete = null)
    {
        if (currentPanel != type)
        {
            onComplete?.Invoke();
            return;
        }

        PlayClickSFX();

        var data = GetPanelData(type);
        if (data == null)
        {
            currentPanel = PanelType.None;
            onComplete?.Invoke();
            return;
        }

        var effect = data.panel?.GetComponent<EffectPanel>();
        if (effect != null)
        {
            effect.CloseDialog(() =>
            {
                data.button?.SetActive(true);
                data.panel?.SetActive(false);
                currentPanel = PanelType.None;
                onComplete?.Invoke();
            });
        }
        else
        {
            data.button?.SetActive(true);
            data.panel?.SetActive(false);
            currentPanel = PanelType.None;
            onComplete?.Invoke();
        }
    }

    private void CloseAllPanels()
    {
        foreach (var data in panels)
        {
            if (data.panel != null) data.panel.SetActive(false);
            if (data.button != null) data.button.SetActive(true);
        }
        currentPanel = PanelType.None;
    }

    public void OpenSetting() => OpenPanel(PanelType.Setting);
    public void CloseSetting() => ClosePanel(PanelType.Setting);

    public void OpenCollection() => OpenPanel(PanelType.Collection);
    public void CloseCollection() => ClosePanel(PanelType.Collection);

    public void OpenLevel() => OpenPanel(PanelType.Level);
    public void CloseLevel() => ClosePanel(PanelType.Level);

    public void OpenCredit() => OpenPanel(PanelType.Credits);
    public void CloseCredit() => ClosePanel(PanelType.Credits);

    public void OpenTutorial() => OpenPanel(PanelType.Tutorial);
    public void CloseTutorial() => ClosePanel(PanelType.Tutorial);

    public void TapToStart()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        PlayClickSFX();
        AudioManager.Instance?.StopBGM();

        var tm = TransitionManager.Instance();
        if (tm != null && transitionSettings != null)
            tm.Transition(nextSceneName, transitionSettings, loadDelay);
        else
        {
            SceneManager.LoadScene(nextSceneName);
            isTransitioning = false;
        }
    }

    // ----- Cleaning & UI Force (tidak berubah) -----
    private void CleanupAllTransitions()
    {
        foreach (var t in FindObjectsByType<Transition>(FindObjectsSortMode.None))
            if (t != null) Destroy(t.gameObject);

        foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj == null) continue;
            string name = obj.name.ToLower();
            if ((name.Contains("transition") || name.Contains("brush")) && obj.GetComponent<TransitionManager>() == null)
                Destroy(obj);
        }

        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas != null && canvas.gameObject.scene.name != gameObject.scene.name)
                canvas.gameObject.SetActive(false);
        }
    }

    private void ResetAllCanvases()
    {
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas == null || canvas.gameObject.scene.name != gameObject.scene.name) continue;
            canvas.sortingOrder = 0;
            canvas.gameObject.SetActive(true);
            var cg = canvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    private void ForceShowMainMenu()
    {
        if (mainMenuContent == null)
        {
            Debug.LogError("[MainMenu] mainMenuContent tidak di-assign!");
            return;
        }

        mainMenuContent.SetActive(true);
        foreach (var cg in mainMenuContent.GetComponentsInChildren<CanvasGroup>(true))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        foreach (Transform child in mainMenuContent.GetComponentsInChildren<Transform>(true))
            child.gameObject.SetActive(true);
    }

    private IEnumerator EnsureMainMenuVisible()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (mainMenuContent == null) yield break;

        foreach (var cg in mainMenuContent.GetComponentsInChildren<CanvasGroup>(true))
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        mainMenuContent.SetActive(true);
    }

    private void PlayEntryAnimation()
    {
        if (mainMenuContent == null) return;
        foreach (var anim in mainMenuContent.GetComponentsInChildren<Animator>(true))
        {
            if (anim == null) continue;
            anim.Rebind();
            anim.Update(0f);
            if (!string.IsNullOrEmpty(entryStateName))
                anim.Play(entryStateName, 0, 0f);
            else
                anim.Play(0, 0, 0f);
            anim.Update(0f);
        }
    }

    private void PlayClickSFX()
    {
        if (clickSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clickSound);
    }
}