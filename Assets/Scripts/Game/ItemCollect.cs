using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemCollect : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text namaText;
    [SerializeField] private TMP_Text jumlahText;
    [SerializeField] private GameObject lockIcon;

    public void Setup(DataHurufAksara data)
    {
        if (data.unlocked)
        {
            icon.sprite = data.gambar;
            namaText.text = data.nama;
            jumlahText.text = data.jumlah + "x";

            lockIcon.SetActive(false);
        }
        else
        {
            namaText.text = "???";
            jumlahText.text = "0x";

            lockIcon.SetActive(true);
        }
    }
}