using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    public Image powerUpImage; // drag Image component dari PowerUp sendiri

    [Header("Warna saat masih terkunci")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // abu-abu gelap
    public Color unlockedColor = Color.white; // warna asli

    void Start()
    {
        SetLocked();
    }

    public void SetLocked()
    {
        powerUpImage.color = lockedColor;
    }

    public void SetUnlocked()
    {
        powerUpImage.color = unlockedColor;
    }
}