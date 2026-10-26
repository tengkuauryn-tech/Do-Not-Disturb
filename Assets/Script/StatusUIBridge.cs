using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUIBridge : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider energySlider;
    public Slider focusSlider;
    public Slider stressSlider;
    public Slider relationSlider;

    [Header("Slider Texts")]
    public TMP_Text energyText;
    public TMP_Text focusText;
    public TMP_Text stressText;
    public TMP_Text relationText;

    [Header("Task Progress Text")]
    public TMP_Text taskProgressText;

    public void UpdateAllBars(float energy, float focus, float stress, float taskProgress, float relationship)
    {
        // 1. Update posisi bar menggunakan fungsi aman milikmu
        AturBarAman(energySlider, energy);
        AturBarAman(focusSlider, focus);
        AturBarAman(stressSlider, stress);
        AturBarAman(relationSlider, relationship);

        // 2. Update teks persentase di dalam bar
        if (energyText != null) energyText.text = Mathf.RoundToInt(energy) + "%";
        if (focusText != null) focusText.text = Mathf.RoundToInt(focus) + "%";
        if (stressText != null) stressText.text = Mathf.RoundToInt(stress) + "%";
        if (relationText != null) relationText.text = Mathf.RoundToInt(relationship) + "%";

        // 3. Update task mingguan
        if (taskProgressText != null)
        {
            taskProgressText.text = Mathf.RoundToInt(taskProgress) + "%";
        }
    }

    private void AturBarAman(Slider slider, float nilai)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = nilai;

            // Memastikan area fill ikut merentang pas mengikuti ukuran slider
            if (slider.fillRect != null)
            {
                RectTransform rect = slider.fillRect.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
        }
    }
}