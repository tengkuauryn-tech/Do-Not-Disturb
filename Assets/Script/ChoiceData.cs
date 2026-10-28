using UnityEngine;

[CreateAssetMenu(fileName = "NewChoice", menuName = "Do Not Disturb/Data Pilihan")]
public class ChoiceData : ScriptableObject
{
    [Header("Teks UI")]
    [TextArea(2, 4)]
    public string teksPilihan;

    [Header("Efek Indikator (Bisa Minus/Plus)")]
    public int task;
    public int stress;
    public int focus;
    public int energy;
    public int relSagara;

    [Header("Sistem Flag")]
    [Tooltip("Kosongkan jika tidak memberikan flag. Contoh isi: hendra_warned")]
    public string setFlag;
    
    [Tooltip("Kosongkan jika opsi selalu muncul. Contoh isi: path_close")]
    public string butuhFlag;
}