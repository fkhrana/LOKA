using UnityEngine;

public class HelperClicker : MonoBehaviour
{
    public PlayerTester playerScript; // Drag objek Player ke slot ini di Inspector

    // Dipanggil saat klik Helper
    void OnMouseDown()
    {
        if (playerScript != null)
        {
            playerScript.TambahHP();
            
            // Opsional: Matikan Helper setelah dia nge-heal (seperti kabur setelah nolong)
            // gameObject.SetActive(false); 
        }
    }
}