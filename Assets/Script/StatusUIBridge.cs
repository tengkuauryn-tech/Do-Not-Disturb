using UnityEngine;
using UnityEngine.UI;

public class StatusUIBridge : MonoBehaviour
{
    [Header("UI Sliders")]
    public Slider energySlider;
    public Slider focusSlider;
    public Slider stressSlider;
    public Slider taskSlider;
    public Slider relationSlider;

    // Fungsi untuk update tampilan bar secara otomatis dari angka Ariqa
    public void UpdateAllBars(float energy, float focus, float stress, float task, float relation)
    {
        energySlider.value = Mathf.Clamp(energy, 0, 100);
        focusSlider.value = Mathf.Clamp(focus, 0, 100);
        stressSlider.value = Mathf.Clamp(stress, 0, 100);
        taskSlider.value = Mathf.Clamp(task, 0, 100);
        relationSlider.value = Mathf.Clamp(relation, 0, 100);
    }
}