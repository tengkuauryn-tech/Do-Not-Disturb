using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    [Header("UI Bridge Reference")]
    public StatusUIBridge uiBridge;

    [Header("Current Stat Values (0-100)")]
    public float energy = 80f;
    public float focus = 50f;
    public float stress = 20f;
    public float taskProgress = 0f;
    public float relationship = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Langsung update visual slider di awal game
        RefreshUI();
    }

    // Fungsi utama buat nambah/ngurangin stat dari pilihan cerita
    public void ChangeStats(float dEnergy, float dFocus, float dStress, float dTask, float dRelation)
    {
        energy += dEnergy;
        focus += dFocus;
        stress += dStress;
        taskProgress += dTask;
        relationship += dRelation;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (uiBridge != null)
        {
            uiBridge.UpdateAllBars(energy, focus, stress, taskProgress, relationship);
        }
    }
}