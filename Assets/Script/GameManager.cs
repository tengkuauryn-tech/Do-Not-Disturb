using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Indikator Utama (Max 100)")]
    public int focus = 50;
    public int taskProgress = 0;
    public int energy = 100;
    public int stress = 0;

    [Header("Indikator Relasi (Max 100)")]
    public int relationshipSagara = 0; // Mulai dari 0 sesuai saran
    public int relationshipTara = 0;
    public int relationshipHendra = 0;

    [Header("Event Flags")]
    public bool hoax_investigated = false;

    [Header("Sambungan UI Indikator")]
    public Image uiImageFocus;
    public Image uiImageTask;
    public Image uiImageEnergy;
    public Image uiImageStress;
    public Image uiImageRelationshipSagara; // Tambahan untuk UI Relasi Sagara

    void Start()
    {
        UpdateVisualUI();
    }

    public void PilihOpsi(int ubahFocus, int ubahTask, int ubahEnergy, int ubahStress)
    {
        focus += ubahFocus;
        taskProgress += ubahTask;
        energy += ubahEnergy;
        stress += ubahStress;

        focus = Mathf.Clamp(focus, 0, 100);
        taskProgress = Mathf.Clamp(taskProgress, 0, 100);
        energy = Mathf.Clamp(energy, 0, 100);
        stress = Mathf.Clamp(stress, 0, 100);

        UpdateVisualUI(); 
    }

    // Fungsi terpisah khusus untuk menambah/mengurangi poin Sagara
    public void UbahRelasiSagara(int poin)
    {
        relationshipSagara += poin;
        relationshipSagara = Mathf.Clamp(relationshipSagara, 0, 100); // Kunci 0-100
        UpdateVisualUI();
        Debug.Log("Relasi Sagara Update: " + relationshipSagara);
    }

    private void UpdateVisualUI()
    {
        if (uiImageFocus != null) uiImageFocus.fillAmount = focus / 100f;
        if (uiImageTask != null) uiImageTask.fillAmount = taskProgress / 100f;
        if (uiImageEnergy != null) uiImageEnergy.fillAmount = energy / 100f;
        if (uiImageStress != null) uiImageStress.fillAmount = stress / 100f;
        
        // Update bar relasi sagara
        if (uiImageRelationshipSagara != null) uiImageRelationshipSagara.fillAmount = relationshipSagara / 100f;
    }

    public void EvaluasiAkhirSemester()
    {
        // SISTEM AIR TERJUN (Mencegah Overlap Ending)
        // 1. Cek dari yang paling buruk (D)
        if (stress >= 90 || energy < 10)
        {
            Debug.Log("Trigger Ending D: Total System Crash! (WORST)");
        }
        // 2. Kalau D tidak kena, baru cek C
        else if (taskProgress < 60 || focus < 40 || stress >= 70)
        {
            Debug.Log("Trigger Ending C: Overwhelmed Noise (BAD)");
        }
        // 3. Kalau aman, cek B (Romance). Syarat dikembalikan ke 30 sesuai GDD
        else if (relationshipSagara >= 30 && taskProgress >= 75 && energy >= 30)
        {
            Debug.Log("Trigger Ending B: Quiet Connection (ROMANCE)");
        }
        // 4. Kalau Romance tidak terpenuhi, cek A (Best)
        else if (taskProgress >= 85 && focus >= 70 && stress <= 40 && hoax_investigated)
        {
            Debug.Log("Trigger Ending A: The Signal Mastery (BEST)");
        }
        else
        {
            Debug.Log("Trigger Ending Default (Belum memenuhi syarat A/B)");
        }
    }
}