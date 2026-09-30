using UnityEngine;
using UnityEngine.UI;

public class AnalyticsConsentUI : MonoBehaviour
{
    private const string ConsentKey = "AksaraAnalytics.Consent.v1";
    private const int NoChoice = 0;
    private const int Granted = 1;
    private const int Denied = 2;

    [SerializeField] private GameObject consentPanel;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    private void Awake()
    {
        int savedChoice = PlayerPrefs.GetInt(ConsentKey, NoChoice);
        bool hasSavedChoice = savedChoice == Granted || savedChoice == Denied;

        if (consentPanel != null)
            consentPanel.SetActive(!hasSavedChoice);

        if (hasSavedChoice)
            AksaraAnalytics.SetAnalyticsConsent(savedChoice == Granted);
    }

    private void OnEnable()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(AcceptAnalytics);

        if (declineButton != null)
            declineButton.onClick.AddListener(DeclineAnalytics);
    }

    private void OnDisable()
    {
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(AcceptAnalytics);

        if (declineButton != null)
            declineButton.onClick.RemoveListener(DeclineAnalytics);
    }

    private void AcceptAnalytics()
    {
        Debug.Log("[AnalyticsConsentUI] Accept button clicked.");
        SaveChoice(Granted, true);
    }

    private void DeclineAnalytics()
    {
        Debug.Log("[AnalyticsConsentUI] Decline button clicked.");
        SaveChoice(Denied, false);
    }

    private void SaveChoice(int choice, bool granted)
    {
        PlayerPrefs.SetInt(ConsentKey, choice);
        PlayerPrefs.Save();

        Debug.Log(
            "[AnalyticsConsentUI] Consent saved to PlayerPrefs => " +
            "key=" + ConsentKey +
            ", choice=" + choice +
            ", granted=" + granted
        );

        AksaraAnalytics.SetAnalyticsConsent(granted);

        if (consentPanel != null)
            consentPanel.SetActive(false);
    }
}