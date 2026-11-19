using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Week7Manager : MonoBehaviour
{
    [Header("Main & Global UI (Sama Kayak Week 5 & 6)")]
    public GameObject week7Section; // Drag object WEEK7
    public GameObject dialogBoxObject;
    public TMP_Text dialogueText;
    public Button invisibleTapButton;
    public Sprite dialogBoxKiranaSprite;
    public Sprite dialogBoxSagaraSprite;
    public Image blackOverlay;
    public TMP_Text weekTitleText;

    [Header("Ending Groups (Parent Empty Object)")]
    public GameObject endingAGroup; // Drag ENDING A
    public GameObject endingBGroup; // Drag ENDING B
    public GameObject endingCGroup; // Drag ENDING C
    public GameObject endingDGroup; // Drag ENDING D

    [Header("Ending A Visuals")]
    public GameObject kamarKosSoreA;    // Kamar Kos Sore
    public GameObject laptopNilaiA;      // Laptop Nilai

    [Header("Ending B Visuals")]
    public GameObject kiranaSagaraB;     // Kirana & Sagara
    public GameObject peganganTanganB;   // Pegangan Tangan

    [Header("Ending C Visuals")]
    public GameObject hpMatiC;           // Hp Mati
    public GameObject notifDikitC;       // Notif Dikit
    public GameObject notifBanyakC;      // Notif Banyak
    public GameObject laptopNilaiC;      // Laptop Nilai
    public GameObject kamarC;            // Kamar C

    [Header("Ending D Visuals")]
    public GameObject hpMatiD;           // Hp MAti
    public GameObject notifEmail1D;      // Notif Email 1
    public GameObject notifEmail2D;      // Notif Email 2
    public GameObject taraTelpD;         // Tara Telp
    public GameObject mejaKosongD;       // Meja Kosong
    public GameObject pegangHpD;         // Pegang hp (PNG terpisah)
    public GameObject kamarGelapD;       // Kamar Gelap D

    [Header("Testing di Inspector (Bisa diubah buat test langsung)")]
    public string defaultEndingTest = "A";

    private bool userTapped, isTyping;

    void Start()
    {
        if (week7Section != null && week7Section.activeInHierarchy)
            StartWeek7(defaultEndingTest);
    }

    // Pintu masuk utama dari script temanmu: week7Manager.StartWeek7("A" / "B" / "C" / "D")
    public void StartWeek7(string endingType)
    {
        if (week7Section != null) week7Section.SetActive(true);

        // Matikan Week 6 jika masih aktif
        Week6Manager w6 = FindFirstObjectByType<Week6Manager>(FindObjectsInactive.Include);
        if (w6 != null) { if (w6.week6Section != null) w6.week6Section.SetActive(false); w6.enabled = false; }

        ResetAllEndings();
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);

        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(false);
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        StopAllCoroutines();
        StartCoroutine(RunWeek7Sequence(endingType.ToUpper().Trim()));
    }

    // Fungsi publik tambahan kalau temanmu mau manggil langsung per ending tanpa parameter string
    public void PlayEndingA() => StartWeek7("A");
    public void PlayEndingB() => StartWeek7("B");
    public void PlayEndingC() => StartWeek7("C");
    public void PlayEndingD() => StartWeek7("D");

    IEnumerator RunWeek7Sequence(string type)
    {
        // 1. BLACK OVERLAY + TITLE "Week 7 - Semester Complete" FADE IN
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.transform.SetAsLastSibling();
            blackOverlay.color = new Color(0, 0, 0, 1f);
        }
        if (weekTitleText != null)
        {
            weekTitleText.gameObject.SetActive(true);
            weekTitleText.transform.SetAsLastSibling();
            weekTitleText.text = "Week 7 - Semester Complete";
            weekTitleText.color = new Color(1, 1, 1, 0f);

            for (float t = 0; t < 0.8f; t += Time.deltaTime)
            {
                weekTitleText.color = new Color(1, 1, 1, t / 0.8f);
                yield return null;
            }
            weekTitleText.color = new Color(1, 1, 1, 1f);
        }

        yield return new WaitForSeconds(2.0f);

        // Fade out title & siapkan visual awal ending di balik layar hitam
        if (weekTitleText != null)
        {
            for (float t = 0; t < 0.6f; t += Time.deltaTime)
            {
                weekTitleText.color = new Color(1, 1, 1, 1f - (t / 0.6f));
                yield return null;
            }
            weekTitleText.gameObject.SetActive(false);
        }

        // Jalankan cabang sesuai hasil ending
        switch (type)
        {
            case "B":
                yield return StartCoroutine(SequenceEndingB());
                break;
            case "C":
                yield return StartCoroutine(SequenceEndingC());
                break;
            case "D":
                yield return StartCoroutine(SequenceEndingD());
                break;
            default:
                yield return StartCoroutine(SequenceEndingA());
                break;
        }
    }

    // ==========================================
    // ENDING A — MASTER OF PRIORITY
    // ==========================================
    IEnumerator SequenceEndingA()
    {
        if (endingAGroup != null) endingAGroup.SetActive(true);
        if (kamarKosSoreA != null) kamarKosSoreA.SetActive(true);
        if (laptopNilaiA != null) laptopNilaiA.SetActive(false);

        // Buka layar hitam pelan masuk ke Kamar Kos Sore
        yield return StartCoroutine(FadeBlack(1f, 0f, 1.0f));
        yield return new WaitForSeconds(1.8f);

        // Kamar Kos > fade out pelan > Visual Laptop
        yield return StartCoroutine(FadeBlack(0f, 1f, 0.9f));
        if (kamarKosSoreA != null) kamarKosSoreA.SetActive(false);
        if (laptopNilaiA != null) laptopNilaiA.SetActive(true);
        yield return StartCoroutine(FadeBlack(1f, 0f, 0.9f));

        yield return StartCoroutine(PlayDialog("Akhirnya lika liku ini udah kelar.", "Kirana"));
        yield return StartCoroutine(PlayDialog("Bedanya, sekarang aku ngerti mana yang harus dibaca, mana yang harus diverifikasi, dan mana yang didiemin.", "Kirana"));

        yield return StartCoroutine(ShowEndingTitle(
            "ENDING A — MASTER OF PRIORITY",
            "Tidak semua sinyal harus langsung direspons—prioritas dan verifikasi adalah kunci mengendalikan keadaan."
        ));
    }

    // ==========================================
    // ENDING B — CLEAR SIGNAL, CLEAR HEART
    // ==========================================
    IEnumerator SequenceEndingB()
    {
        if (endingBGroup != null) endingBGroup.SetActive(true);
        if (kiranaSagaraB != null) kiranaSagaraB.SetActive(true);
        if (peganganTanganB != null) peganganTanganB.SetActive(false);

        // Buka layar hitam masuk ke Kirana & Sagara di taman
        yield return StartCoroutine(FadeBlack(1f, 0f, 1.0f));

        yield return StartCoroutine(PlayDialog("Makasih ya... udah nemenin aku selama satu semester ini, jujur rasanya jadi gampang kalo punya temen sefrekuensi.", "Sagara"));
        yield return StartCoroutine(PlayDialog("E..eh sama sama sagara.", "Kirana"));
        yield return StartCoroutine(PlayDialog("..aku juga ngerasa gampang kalo sama kamu.", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // Fade in pelan pegangan tangan
        if (peganganTanganB != null)
            yield return StartCoroutine(FadeUI(peganganTanganB, 0f, 1f, 1.4f));

        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(ShowEndingTitle(
            "ENDING B — CLEAR SIGNAL, CLEAR HEART",
            "Kejelasan bukan hanya soal menyaring informasi, tapi juga keberanian untuk terbuka pada orang yang tepat."
        ));
    }

    // ==========================================
    // ENDING C — DROWNED IN NOISE
    // ==========================================
    IEnumerator SequenceEndingC()
    {
        if (endingCGroup != null) endingCGroup.SetActive(true);
        if (hpMatiC != null) hpMatiC.SetActive(true);
        if (notifDikitC != null) notifDikitC.SetActive(false);
        if (notifBanyakC != null) notifBanyakC.SetActive(false);
        if (laptopNilaiC != null) laptopNilaiC.SetActive(false);
        if (kamarC != null) kamarC.SetActive(false);

        yield return StartCoroutine(FadeBlack(1f, 0f, 0.9f));

        yield return StartCoroutine(PlayDialog("Haruskah aku menyalakan kembali hp ku", "Kirana"));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // Notif dikit > getar HP standar (sama kaya Week 3 & 6)
        if (hpMatiC != null) hpMatiC.SetActive(false);
        if (notifDikitC != null) notifDikitC.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(VibrateObject(notifDikitC, 0.45f, 15f));
        yield return new WaitForSeconds(0.6f);

        // Notif banyak > getar rusuh HP dibombardir notif!
        if (notifDikitC != null) notifDikitC.SetActive(false);
        if (notifBanyakC != null) notifBanyakC.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(VibrateIntense(notifBanyakC, 0.95f, 24f));
        yield return new WaitForSeconds(0.8f);

        // Fade in laptop nilai
        if (laptopNilaiC != null)
            yield return StartCoroutine(FadeUI(laptopNilaiC, 0f, 1f, 1.0f));
        if (notifBanyakC != null) notifBanyakC.SetActive(false);

        yield return StartCoroutine(PlayDialog("Bodoh... bodoh.", "Kirana"));
        yield return StartCoroutine(PlayDialog("Hasilnya? Tugas ancur, sekarang bingung.", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // Fade out pelan ke kamar kos gelap
        yield return StartCoroutine(FadeBlack(0f, 1f, 1.0f));
        if (laptopNilaiC != null) laptopNilaiC.SetActive(false);
        if (kamarC != null) kamarC.SetActive(true);
        yield return StartCoroutine(FadeBlack(1f, 0f, 1.0f));

        yield return new WaitForSeconds(1.8f);
        yield return StartCoroutine(ShowEndingTitle(
            "ENDING C — DROWNED IN NOISE",
            "Tenggelam dalam kebisingan tanpa menyaring kebenaran hanya akan membuatmu lelah di arah yang salah."
        ));
    }

    // ==========================================
    // ENDING D — DO NOT DISTURB (PERMANENT)
    // ==========================================
    IEnumerator SequenceEndingD()
    {
        if (endingDGroup != null) endingDGroup.SetActive(true);
        if (hpMatiD != null) hpMatiD.SetActive(true);
        if (notifEmail1D != null) notifEmail1D.SetActive(false);
        if (notifEmail2D != null) notifEmail2D.SetActive(false);
        if (taraTelpD != null) taraTelpD.SetActive(false);
        if (mejaKosongD != null) mejaKosongD.SetActive(false);
        if (pegangHpD != null) pegangHpD.SetActive(false);
        if (kamarGelapD != null) kamarGelapD.SetActive(false);

        yield return StartCoroutine(FadeBlack(1f, 0f, 0.9f));

        // Hp mati > getar
        yield return new WaitForSeconds(0.6f);
        yield return StartCoroutine(VibrateObject(hpMatiD, 0.45f, 15f));
        yield return new WaitForSeconds(0.5f);

        // Bg hp notif dikit (Notif Email 1) > getar
        if (hpMatiD != null) hpMatiD.SetActive(false);
        if (notifEmail1D != null) notifEmail1D.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(VibrateObject(notifEmail1D, 0.45f, 15f));
        yield return new WaitForSeconds(0.6f);

        // Bg hp notif banyak (Notif Email 2) > getar
        if (notifEmail1D != null) notifEmail1D.SetActive(false);
        if (notifEmail2D != null) notifEmail2D.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(VibrateObject(notifEmail2D, 0.5f, 17f));
        yield return new WaitForSeconds(0.6f);

        // Tara telpon > getar
        if (notifEmail2D != null) notifEmail2D.SetActive(false);
        if (taraTelpD != null) taraTelpD.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(VibrateObject(taraTelpD, 0.6f, 18f));
        yield return new WaitForSeconds(0.7f);

        // Meja kosong bg > fade in png pegang hp
        if (taraTelpD != null) taraTelpD.SetActive(false);
        if (mejaKosongD != null) mejaKosongD.SetActive(true);
        yield return new WaitForSeconds(0.7f);

        if (pegangHpD != null)
            yield return StartCoroutine(FadeUI(pegangHpD, 0f, 1f, 1.0f));

        yield return StartCoroutine(PlayDialog("HP-ku akhirnya berdering lagi malam ini...", "Kirana"));
        yield return StartCoroutine(PlayDialog("...tapi bahkan untuk mengangkatnya pun, aku sudah nggak punya tenaga.", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // Fade out ke kamar bg
        yield return StartCoroutine(FadeBlack(0f, 1f, 1.0f));
        if (mejaKosongD != null) mejaKosongD.SetActive(false);
        if (pegangHpD != null) pegangHpD.SetActive(false);
        if (kamarGelapD != null) kamarGelapD.SetActive(true);
        yield return StartCoroutine(FadeBlack(1f, 0f, 1.0f));

        yield return new WaitForSeconds(1.8f);
        yield return StartCoroutine(ShowEndingTitle(
            "ENDING D — DO NOT DISTURB (PERMANENT)",
            "Mode 'Do Not Disturb' adalah jeda untuk memulihkan diri, bukan pelarian permanen dari kenyataan."
        ));
    }

    // ==========================================
    // HELPER FUNCTIONS
    // ==========================================
    void ResetAllEndings()
    {
        GameObject[] groups = { endingAGroup, endingBGroup, endingCGroup, endingDGroup };
        foreach (var g in groups) if (g != null) g.SetActive(false);
    }

    IEnumerator ShowEndingTitle(string title, string moralMessage)
    {
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        yield return StartCoroutine(FadeBlack(0f, 1f, 1.2f));

        if (weekTitleText != null)
        {
            weekTitleText.transform.SetAsLastSibling();
            weekTitleText.text = $"{title}\n\n<size=65%><color=#D9D9D9>\"{moralMessage}\"</color></size>";
            weekTitleText.color = new Color(1, 1, 1, 0f);
            weekTitleText.gameObject.SetActive(true);

            for (float t = 0; t < 1.0f; t += Time.deltaTime)
            {
                weekTitleText.color = new Color(1, 1, 1, t / 1.0f);
                yield return null;
            }
            weekTitleText.color = new Color(1, 1, 1, 1f);
        }
    }

    // Animasi getar standar (sama persis kaya Week 3 & Week 6)
    IEnumerator VibrateObject(GameObject target, float duration, float strength)
    {
        if (target == null) yield break;
        Vector3 originalPos = target.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength;
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.transform.localPosition = originalPos;
    }

    // Animasi getar rusuh khusus notif banyak (Ending C)
    IEnumerator VibrateIntense(GameObject target, float duration, float maxStrength)
    {
        if (target == null) yield break;
        Vector3 originalPos = target.transform.localPosition;
        Quaternion originalRot = target.transform.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            // Makin lama makin rusuh sebelum mereda di akhir
            float currentStrength = Mathf.Lerp(maxStrength * 0.6f, maxStrength, Mathf.Sin(progress * Mathf.PI));
            target.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * currentStrength;
            target.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1.8f, 1.8f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.transform.localPosition = originalPos;
        target.transform.localRotation = originalRot;
    }

    IEnumerator FadeUI(GameObject target, float startA, float endA, float dur)
    {
        if (target == null) yield break;
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();
        cg.alpha = startA;
        target.SetActive(true);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(startA, endA, t / dur);
            yield return null;
        }
        cg.alpha = endA;
        if (endA <= 0f) target.SetActive(false);
    }

    IEnumerator FadeBlack(float startA, float endA, float dur)
    {
        if (blackOverlay == null) yield break;
        blackOverlay.transform.SetAsLastSibling();
        blackOverlay.gameObject.SetActive(true);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            blackOverlay.color = new Color(0, 0, 0, Mathf.Lerp(startA, endA, t / dur));
            yield return null;
        }
        blackOverlay.color = new Color(0, 0, 0, endA);
        if (endA <= 0f) blackOverlay.gameObject.SetActive(false);
    }

    IEnumerator PlayDialog(string text, string speaker = "Kirana")
    {
        if (invisibleTapButton != null) { invisibleTapButton.gameObject.SetActive(true); invisibleTapButton.transform.SetAsLastSibling(); }
        if (dialogBoxObject != null)
        {
            Image boxImg = dialogBoxObject.GetComponent<Image>();
            if (boxImg != null)
            {
                if (speaker == "Sagara" && dialogBoxSagaraSprite != null) boxImg.sprite = dialogBoxSagaraSprite;
                else if (dialogBoxKiranaSprite != null) boxImg.sprite = dialogBoxKiranaSprite;
            }
            dialogBoxObject.transform.SetAsLastSibling();
            dialogBoxObject.SetActive(true);
        }

        dialogueText.text = "";
        userTapped = false;
        isTyping = true;

        foreach (char c in text)
        {
            if (!isTyping) break;
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f);
        }

        dialogueText.text = text;
        isTyping = false;
        userTapped = false;
        while (!userTapped) yield return null;
    }

    public void OnUserTap()
    {
        if (isTyping) isTyping = false;
        else userTapped = true;
    }
}