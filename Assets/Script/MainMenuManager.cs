using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject panelCredit;
    public GameObject panelSettings; // Wadah baru untuk panel Settings

    private void Start()
    {
        // Memastikan kedua pop-up tertutup otomatis saat game baru mulai
        if (panelCredit != null) panelCredit.SetActive(false);
        if (panelSettings != null) panelSettings.SetActive(false);
    }

    // ================= BAGIAN CREDIT =================
    public void OpenCredits()
    {
        if (panelCredit != null) panelCredit.SetActive(true);
    }

    public void CloseCredits()
    {
        if (panelCredit != null) panelCredit.SetActive(false);
    }

    // ================= BAGIAN SETTINGS =================
    public void OpenSettings()
    {
        if (panelSettings != null) panelSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        if (panelSettings != null) panelSettings.SetActive(false);
    }

    // ================= BAGIAN GAMEPLAY =================
    public void PlayGame()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}