using UnityEngine;

public class CollectManager : MonoBehaviour
{
    [SerializeField] private ItemCollect[] items;
    [SerializeField] private Sprite[] sprites;

    private DataHurufAksara[] data;

    private void Start()
    {
        // Ambil jumlah terkecil supaya tidak error
        int jumlahData = Mathf.Min(items.Length, sprites.Length);

        data = new DataHurufAksara[jumlahData];

        for (int i = 0; i < jumlahData; i++)
        {
            data[i] = new DataHurufAksara();

            data[i].nama = "Aksara " + (i + 1);
            data[i].gambar = sprites[i];
            data[i].unlocked = false;
            data[i].jumlah = 0;

            items[i].Setup(data[i]);
        }

        Debug.Log($"Jumlah item: {items.Length}");
        Debug.Log($"Jumlah sprite: {sprites.Length}");
    }

    public void UnlockAksara(int index)
    {
        if (index < 0 || index >= data.Length)
        {
            Debug.LogError("Index tidak valid!");
            return;
        }

        data[index].unlocked = true;
        data[index].jumlah++;

        items[index].Setup(data[index]);
    }
}