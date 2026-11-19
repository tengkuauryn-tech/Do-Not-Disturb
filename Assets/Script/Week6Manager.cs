using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Week6Manager : MonoBehaviour
{
    [Header("Main & Visuals")]
    public GameObject week6Section;
    public GameObject bgKoridor;
    public GameObject hpLockscreenObject;
    public GameObject hpGroupChatObject;

    [Header("Group Chat & Avatars")]
    public TMP_Text contactNameText;
    public Image profilePicImage;
    public Sprite groupProfileSprite;
    public Transform chatContainer;
    public GameObject groupLeftBubblePrefab;
    public GameObject fileBocoranPrefab;
    public Sprite sagaraProfileSprite;
    public Sprite anggota1ProfileSprite;
    public Sprite anggota2ProfileSprite;

    [Header("Global UI (Sama Kayak Week 5)")]
    public GameObject dialogBoxObject;
    public TMP_Text dialogueText;
    public Button invisibleTapButton;
    public Sprite dialogBoxKiranaSprite;
    public Sprite dialogBoxSagaraSprite;
    public Sprite dialogBoxPakHendraSprite;
    public Image blackOverlay;
    public TMP_Text weekTitleText;

    [Header("Choice Panel & DND (Tampil Setelah Dialog Kirana)")]
    public CanvasGroup choicePanelWeek6;
    public Button buttonA;
    public Button buttonB;
    public Button buttonC;
    public Button dndToggleButton;
    public Image dndToggleImage;
    public Sprite dndOffSprite;
    public Sprite dndOnSprite;
    public float delayBeforeFirstButton = 0.2f;
    public float delayBetweenButtons = 0.15f;
    public float buttonFadeDuration = 0.3f;

    [Header("Visual Layers - Cabang Pilihan")]
    public GameObject bgDepanRuangDosen;    // Option A (1): Kirana depan pintu ruang dosen
    public GameObject bgDalamRuangDosen;    // Option A (2): Ruangan dosen (tahan 2 detik)
    public GameObject pakHendraSpriteObj;   // Option A (3): Pak Hendra PNG (fade in)

    public GameObject bgKisiKisiGaResmi;    // Option B (1 & 3): Laptop notepad kisi-kisi ga resmi
    public GameObject bgJam0215;            // Option B (2): Jam pukul 02.15
    public GameObject bgElearningPortal;    // Option B (4): BG portal e-learning
    public GameObject bgKisiKisiResmi;      // Option B (5): BG kisi-kisi resmi

    public GameObject bgSagaraSayHi;        // Option C (1): BG Sagara say hi ("Eh hai kir")
    public GameObject bgKelasSagara;        // Option C (2): BG Kelas kosong
    public GameObject sagaraSpriteObj;      // Option C (3): Sagara PNG terpisah (fade in di atas BG Kelas)

    public GameObject bgBelajarDnd;         // DND: BG Kirana belajar / nulis di buku

    private bool userTapped, isTyping;
    private bool isDndActive = false;
    private float lastTapTime = 0f;

    void Start()
    {
        if (week6Section != null && week6Section.activeInHierarchy)
            StartWeek6();
    }

    public void StartWeek6()
    {
        if (week6Section != null) week6Section.SetActive(true);

        // Matikan Week 5
        Week5Manager w5 = FindFirstObjectByType<Week5Manager>(FindObjectsInactive.Include);
        if (w5 != null) { if (w5.week5Section != null) w5.week5Section.SetActive(false); w5.enabled = false; }

        // Reset UI awal
        HideBranchVisuals();
        ResetChoicePanel();
        isDndActive = false;
        if (dndToggleButton != null) dndToggleButton.interactable = true;
        if (dndToggleImage != null && dndOffSprite != null) dndToggleImage.sprite = dndOffSprite;

        if (bgKoridor != null) bgKoridor.SetActive(true);
        if (hpLockscreenObject != null) hpLockscreenObject.SetActive(false);
        if (hpGroupChatObject != null) hpGroupChatObject.SetActive(false);
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);

        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(false);
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        // Nyalakan Black Overlay + Judul Week 6
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
            weekTitleText.text = "Week 6 - Peak Overload & Crisis";
            weekTitleText.color = new Color(1, 1, 1, 1f);
        }

        StopAllCoroutines();
        StartCoroutine(RunWeek6());
    }

    IEnumerator RunWeek6()
    {
        // 1. WEEK 6 OVERLAY -> FADE OUT
        yield return new WaitForSeconds(2.0f);
        if (weekTitleText != null) weekTitleText.gameObject.SetActive(false);
        if (blackOverlay != null)
        {
            for (float t = 0; t < 0.8f; t += Time.deltaTime)
            {
                blackOverlay.color = new Color(0, 0, 0, 1f - (t / 0.8f));
                yield return null;
            }
            blackOverlay.gameObject.SetActive(false);
        }

        // 2. KORIDOR
        yield return new WaitForSeconds(2.0f);

        // 3. FADE IN LAYAR LOCKSCREEN
        if (hpLockscreenObject != null)
        {
            CanvasGroup cg = hpLockscreenObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = hpLockscreenObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            hpLockscreenObject.SetActive(true);
            hpLockscreenObject.transform.SetAsLastSibling();

            for (float t = 0; t < 0.7f; t += Time.deltaTime)
            {
                cg.alpha = t / 0.7f;
                yield return null;
            }
            cg.alpha = 1f;
            yield return new WaitForSeconds(1f);

            // 4. ANIMASI GETAR -> PINDAH KE LAYAR GRUP CHAT
            Vector3 posAsli = hpLockscreenObject.transform.localPosition;
            for (float t = 0; t < 0.45f; t += Time.deltaTime)
            {
                hpLockscreenObject.transform.localPosition = posAsli + (Vector3)Random.insideUnitCircle * 14f;
                yield return null;
            }
            hpLockscreenObject.transform.localPosition = posAsli;
            hpLockscreenObject.SetActive(false);
        }

        // Buka HP Grup Chat
        if (contactNameText != null) contactNameText.text = "TMD 52 ASIK";
        if (profilePicImage != null && groupProfileSprite != null) profilePicImage.sprite = groupProfileSprite;
        if (chatContainer != null) foreach (Transform c in chatContainer) Destroy(c.gameObject);

        if (hpGroupChatObject != null)
        {
            hpGroupChatObject.SetActive(true);
            if (!hpGroupChatObject.activeInHierarchy && hpGroupChatObject.transform.parent != null)
                hpGroupChatObject.transform.parent.gameObject.SetActive(true);

            CanvasGroup cgGroup = hpGroupChatObject.GetComponent<CanvasGroup>();
            if (cgGroup != null) cgGroup.alpha = 1f;

            hpGroupChatObject.transform.SetAsLastSibling();
        }

        // 5. CHATAN GRUP
        yield return StartCoroutine(SpawnChat("Guys, katanya soal UAS besok udah bocor nih. Aku ada file soalnya, mau share?", anggota1ProfileSprite));
        if (fileBocoranPrefab != null)
            yield return StartCoroutine(SpawnChat("", anggota1ProfileSprite, fileBocoranPrefab));
        yield return StartCoroutine(SpawnChat("Hah serius? Kirim dong, aku belum belajar", anggota2ProfileSprite));
        yield return StartCoroutine(SpawnChat("Ini beneran soal UAS atau cuma prediksi? Jangan asal percaya dulu.", sagaraProfileSprite));

        yield return new WaitForSeconds(0.2f);

        // 6. KIRANA NGOMONG (Tunggu selesai dulu baru munculkan choice)
        yield return StartCoroutine(PlayDialog("Masa sih? Kok aku curiga ya.", "Kirana"));
        yield return StartCoroutine(PlayDialog("Apa karena lagi UAS, banyak kabar ga pasti kayak gini yang tersebar, perlu aku cek ga ya?", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // 7. MUNCULKAN CHOICE PANEL
        ShowChoices();
    }

    // ==========================================
    // CHOICE PANEL & PERPINDAHAN SCENE
    // ==========================================
    void ShowChoices()
    {
        if (buttonA != null) { buttonA.onClick.RemoveAllListeners(); buttonA.onClick.AddListener(OnSelectOptionA); }
        if (buttonB != null) { buttonB.onClick.RemoveAllListeners(); buttonB.onClick.AddListener(OnSelectOptionB); }
        if (buttonC != null) { buttonC.onClick.RemoveAllListeners(); buttonC.onClick.AddListener(OnSelectOptionC); }
        if (dndToggleButton != null) { dndToggleButton.onClick.RemoveAllListeners(); dndToggleButton.onClick.AddListener(OnSelectDND); }

        if (choicePanelWeek6 != null)
        {
            choicePanelWeek6.transform.SetAsLastSibling();
            choicePanelWeek6.gameObject.SetActive(true);
            choicePanelWeek6.alpha = 1f;
            choicePanelWeek6.interactable = true;
            choicePanelWeek6.blocksRaycasts = true;

            bool isDistant = Week2Manager.isPathDistant;

            List<CanvasGroup> activeButtonList = new List<CanvasGroup>();

            CanvasGroup cgDnd = GetOrAddCanvasGroup(dndToggleButton);
            if (cgDnd != null) activeButtonList.Add(cgDnd);

            CanvasGroup cgA = GetOrAddCanvasGroup(buttonA);
            if (cgA != null) activeButtonList.Add(cgA);

            CanvasGroup cgB = GetOrAddCanvasGroup(buttonB);
            if (cgB != null) activeButtonList.Add(cgB);

            // Option C (diskusi dengan Sagara) hanya muncul jika TIDAK distant
            if (buttonC != null)
            {
                if (isDistant)
                {
                    buttonC.gameObject.SetActive(false);
                }
                else
                {
                    CanvasGroup cgC = GetOrAddCanvasGroup(buttonC);
                    if (cgC != null) activeButtonList.Add(cgC);
                }
            }

            foreach (CanvasGroup cg in activeButtonList)
            {
                cg.gameObject.SetActive(true);
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }

            StartCoroutine(AnimateButtonsStaggered(activeButtonList.ToArray()));
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(Button btn)
    {
        if (btn == null) return null;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    IEnumerator AnimateButtonsStaggered(CanvasGroup[] targetButtons)
    {
        yield return new WaitForSeconds(delayBeforeFirstButton);
        if (targetButtons == null) yield break;

        foreach (CanvasGroup cg in targetButtons)
        {
            if (cg != null && cg.gameObject.activeSelf)
            {
                float t = 0f;
                while (t < buttonFadeDuration)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
                    yield return null;
                }
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                yield return new WaitForSeconds(delayBetweenButtons);
            }
        }
    }

    public void OnSelectOptionA() { ResetChoicePanel(); StopAllCoroutines(); StartCoroutine(RunOptionA()); }
    public void OnSelectOptionB() { ResetChoicePanel(); StopAllCoroutines(); StartCoroutine(RunOptionB()); }
    public void OnSelectOptionC() { ResetChoicePanel(); StopAllCoroutines(); StartCoroutine(RunOptionC()); }

    public void OnSelectDND()
    {
        if (isDndActive) return;
        isDndActive = true;
        if (dndToggleButton != null) dndToggleButton.interactable = false;
        if (dndToggleImage != null && dndOnSprite != null) dndToggleImage.sprite = dndOnSprite;

        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunOptionDND());
    }

    // --- OPTION A ---
    IEnumerator RunOptionA()
    {
        if (hpGroupChatObject != null) hpGroupChatObject.SetActive(false);
        if (bgKoridor != null) bgKoridor.SetActive(false);
        HideBranchVisuals();

        if (bgDepanRuangDosen != null) bgDepanRuangDosen.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        if (bgDepanRuangDosen != null) bgDepanRuangDosen.SetActive(false);
        if (bgDalamRuangDosen != null) bgDalamRuangDosen.SetActive(true);
        yield return new WaitForSeconds(2.0f);

        if (pakHendraSpriteObj != null)
            yield return StartCoroutine(FadeUI(pakHendraSpriteObj, 0f, 1f, 0.7f));

        yield return StartCoroutine(PlayDialog("Selamat pagi, Bapak. Mohon maaf mengganggu waktunya, saya ingin mengonfirmasi, apakah benar UAS dimajukan besok dan formatnya esai semua, Pak?", "Kirana"));
        yield return StartCoroutine(PlayDialog("Itu informasi palsu. Bagus kamu bertanya langsung sebelum ikut panik.", "Pak Hendra"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // PINDAH KE WEEK 7 (Otomatis setelah Week 6 selesai)
        TriggerWeek7("A");
    }

    // --- OPTION B ---
    IEnumerator RunOptionB()
    {
        yield return StartCoroutine(FadeBlack(0f, 1f, 0.5f));
        if (hpGroupChatObject != null) hpGroupChatObject.SetActive(false);
        if (bgKoridor != null) bgKoridor.SetActive(false);
        HideBranchVisuals();

        if (bgKisiKisiGaResmi != null) bgKisiKisiGaResmi.SetActive(true);
        yield return StartCoroutine(FadeBlack(1f, 0f, 0.5f));

        yield return StartCoroutine(PlayDialog("Kalau benar besok UAS, aku harus belajar semuanya malam ini.", "Kirana"));

        if (bgKisiKisiGaResmi != null) bgKisiKisiGaResmi.SetActive(false);
        if (bgJam0215 != null) bgJam0215.SetActive(true);
        yield return StartCoroutine(PlayDialog("Udah malem banget. Aku bahkan belum belajar semua.", "Kirana"));

        if (bgJam0215 != null) bgJam0215.SetActive(false);
        if (bgKisiKisiGaResmi != null) bgKisiKisiGaResmi.SetActive(true);
        yield return StartCoroutine(PlayDialog("Hmm, di web kampus ada kisi-kisinya ga ya?", "Kirana"));

        if (bgKisiKisiGaResmi != null) bgKisiKisiGaResmi.SetActive(false);
        if (bgElearningPortal != null) bgElearningPortal.SetActive(true);
        yield return StartCoroutine(PlayDialog("Hah, tadi kisi-kisi informasi yang dikelas itu palsu", "Kirana"));

        if (bgElearningPortal != null) bgElearningPortal.SetActive(false);
        if (bgKisiKisiResmi != null) bgKisiKisiResmi.SetActive(true);
        yield return StartCoroutine(PlayDialog("Aku udah begadang berati aku menghabiskan waktu mempelajari materi yang salah.", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // PINDAH KE WEEK 7
        TriggerWeek7("C"); // Jalur C/D menyesuaikan hasil stat temanmu nanti
    }

    // --- OPTION C ---
    IEnumerator RunOptionC()
    {
        if (hpGroupChatObject != null) hpGroupChatObject.SetActive(false);
        if (bgKoridor != null) bgKoridor.SetActive(false);
        HideBranchVisuals();

        if (bgSagaraSayHi != null) bgSagaraSayHi.SetActive(true);
        yield return StartCoroutine(PlayDialog("Eh hai kir", "Sagara"));

        if (bgSagaraSayHi != null) bgSagaraSayHi.SetActive(false);
        if (bgKelasSagara != null) bgKelasSagara.SetActive(true);
        if (sagaraSpriteObj != null)
            yield return StartCoroutine(FadeUI(sagaraSpriteObj, 0f, 1f, 0.5f));

        yield return StartCoroutine(PlayDialog("kamu udah baca chat aku?", "Sagara"));
        yield return StartCoroutine(PlayDialog("Jangan panik dulu, Sag. Coba kita cek pengumuman resmi di portal kampus.", "Kirana"));
        yield return StartCoroutine(PlayDialog("Oiya benar. Jangan langsung percaya sebelum ada verifikasi.", "Sagara"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // PINDAH KE WEEK 7 (Contoh mengarah ke Ending B / Romance)
        TriggerWeek7("B");
    }

    // --- DND ---
    IEnumerator RunOptionDND()
    {
        if (hpGroupChatObject != null)
            yield return StartCoroutine(FadeUI(hpGroupChatObject, 1f, 0f, 0.5f));

        if (bgKoridor != null) bgKoridor.SetActive(false);
        HideBranchVisuals();

        if (bgBelajarDnd != null)
            yield return StartCoroutine(FadeUI(bgBelajarDnd, 0f, 1f, 0.6f));

        yield return StartCoroutine(PlayDialog("Notifikasi grup kumatikan dulu. Aku harus menyelesaikan tugas yang sudah mendekati deadline. Nanti setelah selesai, aku akan mengecek informasi UTS melalui portal resmi.", "Kirana"));
        yield return StartCoroutine(PlayDialog("Terserah apakah ini berita bohong atau tidak, aku harus menyelesaikan tugas ini sebelum tenggat waktu.", "Kirana"));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // PINDAH KE WEEK 7
        TriggerWeek7("D");
    }

    // Fungsi helper buat manggil Week7Manager
    void TriggerWeek7(string endingType)
    {
        Week7Manager w7 = FindFirstObjectByType<Week7Manager>(FindObjectsInactive.Include);
        if (w7 != null)
        {
            w7.StartWeek7(endingType);
        }
        else
        {
            Debug.LogWarning("Week7Manager tidak ditemukan di scene!");
        }
    }

    // ==========================================
    // HELPER FUNCTIONS
    // ==========================================
    void HideBranchVisuals()
    {
        GameObject[] arr = { bgDepanRuangDosen, bgDalamRuangDosen, pakHendraSpriteObj, bgKisiKisiGaResmi, bgJam0215, bgElearningPortal, bgKisiKisiResmi, bgSagaraSayHi, bgKelasSagara, sagaraSpriteObj, bgBelajarDnd };
        foreach (var obj in arr) if (obj != null) obj.SetActive(false);
    }

    void ResetChoicePanel()
    {
        if (choicePanelWeek6 == null) return;
        choicePanelWeek6.alpha = 0f;
        choicePanelWeek6.gameObject.SetActive(false);
        choicePanelWeek6.interactable = false;
        choicePanelWeek6.blocksRaycasts = false;
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

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTapButton.interactable = true;
            Image img = invisibleTapButton.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            invisibleTapButton.transform.SetAsLastSibling();
        }
    }

    IEnumerator SpawnChat(string msg, Sprite avatar, GameObject customPrefab = null)
    {
        GameObject prefab = customPrefab != null ? customPrefab : groupLeftBubblePrefab;
        if (chatContainer == null || prefab == null) yield break;

        EnsureTapButtonActive();
        userTapped = false;

        GameObject bubble = Instantiate(prefab, chatContainer, false);
        Transform avatarHolder = bubble.transform.Find("HeaderPanel (1)");
        if (avatarHolder != null && avatar != null)
        {
            Image[] imgs = avatarHolder.GetComponentsInChildren<Image>(true);
            if (imgs.Length > 0) imgs[imgs.Length - 1].sprite = avatar;
        }

        if (customPrefab == null)
        {
            TMP_Text txt = bubble.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = "";
                isTyping = true;
                foreach (char c in msg)
                {
                    if (!isTyping) break;
                    txt.text += c;
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainer.GetComponent<RectTransform>());
                    yield return new WaitForSeconds(0.025f);
                }
                txt.text = msg;
                isTyping = false;
            }
        }
        else
        {
            isTyping = false;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainer.GetComponent<RectTransform>());

        // Tunggu pemain tap layar sebelum lanjut ke pesan chat berikutnya
        userTapped = false;
        while (!userTapped) yield return null;
        userTapped = false;
    }

    IEnumerator PlayDialog(string text, string speaker = "Kirana")
    {
        EnsureTapButtonActive();
        if (dialogBoxObject != null)
        {
            Image boxImg = dialogBoxObject.GetComponent<Image>();
            if (boxImg != null)
            {
                if (speaker == "Sagara" && dialogBoxSagaraSprite != null) boxImg.sprite = dialogBoxSagaraSprite;
                else if (speaker == "Pak Hendra" && dialogBoxPakHendraSprite != null) boxImg.sprite = dialogBoxPakHendraSprite;
                else if (dialogBoxKiranaSprite != null) boxImg.sprite = dialogBoxKiranaSprite;
            }
            dialogBoxObject.transform.SetAsLastSibling();
            dialogBoxObject.SetActive(true);
        }

        EnsureTapButtonActive();
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
        userTapped = false;
    }

    public void OnUserTap()
    {
        if (isTyping) isTyping = false;
        else userTapped = true;
    }
}