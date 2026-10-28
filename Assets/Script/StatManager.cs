using System.Collections.Generic;
using UnityEngine;

public enum TipeEnding { EndingA, EndingB, EndingC, EndingD }

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("UI Bridge Reference")]
    public StatusUIBridge uiBridge;

    [Header("Current Stat Values (0-100)")]
    public float energy = 70f; 
    public float focus = 50f;
    public float stress = 20f;
    public float taskProgress = 0f;
    public float relationship = 0f;

    [Header("Catatan Perjalanan (Flags)")]
    public List<string> flagsPemain = new List<string>();

    private void Awake()
    {
        // Sistem 'Destroy' dihapus agar _StatManager tidak menghilang saat di-Play
        Instance = this;
    }

    private void Start()
    {
        // Langsung update visual slider di awal game
        RefreshUI();
    }

    // Fungsi menerima data dari ScriptableObject (ChoiceData)
    public void TerapkanPilihan(ChoiceData pilihan)
    {
        if (pilihan == null) return;

        // Tambahkan nilai dan pastikan tidak tembus batas 0 - 100 (Clamp)
        energy = Mathf.Clamp(energy + pilihan.energy, 0f, 100f);
        focus = Mathf.Clamp(focus + pilihan.focus, 0f, 100f);
        stress = Mathf.Clamp(stress + pilihan.stress, 0f, 100f);
        taskProgress = Mathf.Clamp(taskProgress + pilihan.task, 0f, 100f);
        relationship = Mathf.Clamp(relationship + pilihan.relSagara, -100f, 100f); // Relasi bisa minus

        // Catat Flag jika opsi tersebut memiliki Flag
        if (!string.IsNullOrEmpty(pilihan.setFlag) && !flagsPemain.Contains(pilihan.setFlag))
        {
            flagsPemain.Add(pilihan.setFlag);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (uiBridge != null)
        {
            uiBridge.UpdateAllBars(energy, focus, stress, taskProgress, relationship);
        }
        else
        {
            Debug.LogWarning("[StatManager] Kabel UI Bridge di Inspector lepas atau belum dipasang!");
        }
    }

    // Fungsi untuk mengecek apakah pemain punya flag tertentu
    public bool PunyaFlag(string namaFlag)
    {
        return flagsPemain.Contains(namaFlag);
    }

    // Fungsi evaluasi penentu Ending di Week 7
    public TipeEnding EvaluasiAkhirSemester()
    {
        if (stress >= 70f || energy <= 20f) return TipeEnding.EndingD;
        if (taskProgress >= 80f && focus >= 80f && stress <= 40f && PunyaFlag("hoax_investigated")) return TipeEnding.EndingA;
        if (relationship >= 30f && taskProgress >= 65f && energy >= 30f) return TipeEnding.EndingB;
        
        return TipeEnding.EndingC;
    }
}