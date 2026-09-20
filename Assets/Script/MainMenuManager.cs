using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Dipanggil saat tombol START diklik (Mulai game baru)
    public void PlayGame()
    {
        Debug.Log("Mulai Game Baru");
        // Opsional: Hapus save data lama jika ingin reset total
        // PlayerPrefs.DeleteAll(); 
        
        SceneManager.LoadScene("Gameplay");
    }

    // Dipanggil saat tombol CONTINUE diklik (Melanjutkan progres)
    public void ContinueGame()
    {
        Debug.Log("Melanjutkan Game");
        // Nanti bisa dimuat data Week terakhir yang disimpan
        SceneManager.LoadScene("Gameplay");
    }

    public void OpenCredits()
    {
        Debug.Log("Membuka Credit");
    }

    public void OpenSettings()
    {
        Debug.Log("Membuka Setting");
    }
}