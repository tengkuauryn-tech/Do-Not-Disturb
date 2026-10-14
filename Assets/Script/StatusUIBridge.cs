using UnityEngine;
using UnityEngine.UI;
using TMPro; // Nambahin namespace TextMeshPro

public class StatusUIBridge : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider energySlider;
    public Slider focusSlider;
    public Slider stressSlider;
    public Slider relationSlider;

    [Header("Task Progress Text")]
    public TMP_Text taskProgressText; 

    
    public void UpdateAllBars(float energy, float focus, float stress, float task, float relation)
    {
        if (energySlider != null) energySlider.value = Mathf.Clamp(energy, 0, 100);
        if (focusSlider != null) focusSlider.value = Mathf.Clamp(focus, 0, 100);
        if (stressSlider != null) stressSlider.value = Mathf.Clamp(stress, 0, 100);
        if (relationSlider != null) relationSlider.value = Mathf.Clamp(relation, 0, 100);

        // Update teks persentase Task Progress
        if (taskProgressText != null)
        {
            taskProgressText.text = $"{Mathf.Clamp(task, 0, 100):F0}%";
        }
    }
}