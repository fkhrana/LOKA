using UnityEngine;
using TMPro;

public class DotController : MonoBehaviour
{
    public int dotNumber; // nomor urut titik ini (1, 2, 3, dst)
    public TextMeshPro numberLabel; // teks angka yang tampil di atas titik

    public void Setup(int number)
    {
        dotNumber = number;
        if (numberLabel != null)
            numberLabel.text = number.ToString();
    }
}
