using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Week1Manager : MonoBehaviour
{
    #region CSV & MAIN CONTAINERS
    [Header("CSV & Week Transition")]
    public string csvFileName = "WEEK_1_SCRIPT";
    public GameObject week1Section;
    public Week2Manager week2Manager;
    public GameObject statusPanel;
    public SpotlightTutorial spotlightTutorial; // <-- Slot baru untuk Tutorial

    [Header("Phone Containers & Dynamic Chat Prefabs")]
    public GameObject phoneMainListChat;
    public GameObject phoneViewInsideChat;
    public GameObject phoneViewScreenOff;
    public Transform insidePhoneTransform;
    public Transform chatContainer;             // Drag object ChatContainer (kosong)
    public GameObject leftBubblePrefab;         // Prefab chat 1-on-1 kiri (Pak Hendra / Tara)
    public GameObject rightBubblePrefab;        // Prefab chat kanan (Kirana)
    public GameObject groupLeftBubblePrefab;    // Prefab chat grup kiri (dengan avatar)
    public float phoneZoomDuration = 0.35f;
    #endregion

    #region CHOICE PANELS
    [Header("Choice Panel 1 (Scene 01.01)")]
    public CanvasGroup choicePanelCanvasGroup;
    public CanvasGroup[] optionCanvasGroups;
    public float delayBeforeFirstButton = 0.4f;
    public float delayBetweenButtons = 0.2f;
    public float buttonFadeDuration = 0.3f;

    [Header("Choice Panel 2 - Tara (Scene 01.02)")]
    public CanvasGroup choicePanelTaraCanvasGroup;
    public CanvasGroup[] optionTaraCanvasGroups;

    [Header("Choice Panel 3 - Kelas (Scene 01.03)")]
    public CanvasGroup choicePanel1_3CanvasGroup;
    public CanvasGroup[] option1_3CanvasGroups;
    #endregion

    #region SCENE 1.3 & DESK VISUALS
    [Header("Scene 1.3 - Background & Overlays")]
    public Image mainBackgroundImage;
    public Image actionOverlayImage;
    public Sprite bgRuangKelas;
    public Sprite spriteMcCatat;
    public Sprite spriteMcLihatHp;
    public Sprite spriteMcAngkatTangan;

    [Header("Scene 1.3 - Pak Hendra Poses")]
    public Image hendraCharacterImage;
    public Sprite hendraNormalSprite;
    public Sprite hendraPraiseSprite;
    public Sprite hendraSilentStareSprite;
    public Sprite hendraTegasSprite;

    [Header("Option C Desk & Interlude Assets")]
    public Image blackOverlay;
    public TMP_Text interludeText;
    public GameObject tablePhoneView;
    public GameObject tablePhoneNotifOn;
    #endregion

    #region GLOBAL UI, HEADER, DND & AUDIO
    [Header("Global Interactive Controls")]
    public Button invisibleTapButton;
    public GameObject dialogPanel;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public Image dialogBoxBackgroundImage;

    [Header("DialogBox Sprites Per Character")]
    public Sprite dialogBoxKiranaSprite;
    public Sprite dialogBoxHendraSprite;
    public Sprite dialogBoxTaraSprite;

    [Header("Header & Avatar Elements")]
    public TMP_Text headerNameText;
    public Image headerProfileImage;
    public Sprite dosenProfileSprite;
    public Sprite grupProfileSprite;
    public Sprite taraProfileSprite;
    public Sprite anggotaGrupSprite;          // Avatar anggota grup lainnya di Option B

    [Header("DND Mechanic Controls")]
    public Button dndToggleButton;
    public Image dndToggleImage;
    public Sprite dndOnSprite;
    public Sprite dndOffSprite;
    public GameObject dndStatusIcon;
    [HideInInspector] public bool isDndActive = false;

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip messageSFX;
    public AudioClip beepSFX;

    [Header("Typing Settings")]
    public float typingSpeed = 0.04f;
    public float initialDelayBeforeDialogue = 1.0f;
    public float dialogueFadeInDuration = 0.3f;
    #endregion

    private bool userTapped, isTyping;
    private string fullTextHolder = "";
    private List<Dictionary<string, string>> csvData;
    private Coroutine activeSceneCoroutine;
    private bool hasPlayedTutorial = false;

    #region INITIALIZATION
    public void StartWeek1()
    {
        if (week1Section != null) week1Section.SetActive(true);
        if (statusPanel != null) statusPanel.SetActive(true);

        hasPlayedTutorial = false;
        csvData = CSVReader.Read(csvFileName);

        EnsureTapButtonActive();
        if (invisibleTapButton != null)
        {
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        ResetToMainList();
        RunBranch(PlaySceneFromCSV("SCENE_01_01"));
    }

    private void RunBranch(IEnumerator routine)
    {
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(routine);
    }
    #endregion

    #region MAIN SCENE RUNNER (CSV)
    IEnumerator PlaySceneFromCSV(string targetSceneID)
    {
        if (targetSceneID == "SCENE_01_02")
        {
            HideDialogBoxInstant();
            yield return StartCoroutine(RunScene0102TaraSequence());
            yield break;
        }

        if (targetSceneID == "SCENE_01_03")
        {
            HideDialogBoxInstant();
            yield return StartCoroutine(RunScene0103HendraSequence());
            yield break;
        }

        for (int i = 0; i < csvData.Count; i++)
        {
            if (csvData[i]["SceneID"] != targetSceneID) continue;

            string nodeType = csvData[i]["NodeType"];
            string speaker = csvData[i]["Speaker"];
            string text = csvData[i]["DialogueText"];
            string trigger = csvData[i]["TriggerAction"];
            string sfx = csvData[i]["SFX"];
            string nextScene = csvData[i]["NextSceneID"];

            if (sfx == "beep") PlaySFX(beepSFX);
            else if (sfx == "message") PlaySFX(messageSFX);

            if (trigger == "SHOW_CHAT_DOSEN") { HideDialogBoxInstant(); yield return StartCoroutine(RunOptionASequence()); yield break; }
            if (trigger == "SHOW_CHAT_GRUP") { HideDialogBoxInstant(); yield return StartCoroutine(RunOptionBSequence()); yield break; }
            if (trigger == "SHOW_CHAT_TARA" || trigger == "SHOW_WA_INTERFACE") { HideDialogBoxInstant(); yield return StartCoroutine(RunScene0102TaraSequence()); yield break; }
            if (trigger == "FLIP_HP") { HideDialogBoxInstant(); yield return StartCoroutine(RunOptionCSequence()); yield break; }
            if (trigger == "DND_ACTIVE") { HideDialogBoxInstant(); ToggleDNDState(); yield break; }

            if (nodeType == "DIALOGUE" && !string.IsNullOrEmpty(text))
            {
                if (targetSceneID == "SCENE_01_01") yield return new WaitForSeconds(initialDelayBeforeDialogue);
                yield return StartCoroutine(ShowDialogueWithTyping(speaker, text));
                yield return StartCoroutine(WaitForTap());
                if (trigger.StartsWith("TRIGGER_CHOICE")) HideDialogBoxInstant();
            }

            if (trigger.StartsWith("TRIGGER_CHOICE"))
            {
                HideDialogBoxInstant();
                if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

                // Tunggu semua tombol pilihan selesai muncul dulu
                yield return StartCoroutine(AnimateButtonsStaggered(optionCanvasGroups));

                // Jalankan Spotlight Tutorial sekali setelah pilihan muncul
                if (!hasPlayedTutorial && spotlightTutorial != null)
                {
                    hasPlayedTutorial = true;
                    yield return StartCoroutine(spotlightTutorial.PlayTutorial());

                    // Kembalikan listener Invisible Tap Area ke fungsi dialog normal
                    if (invisibleTapButton != null)
                    {
                        invisibleTapButton.onClick.RemoveAllListeners();
                        invisibleTapButton.onClick.AddListener(OnUserTap);
                        invisibleTapButton.gameObject.SetActive(false);
                    }
                }

                yield break;
            }

            if (!string.IsNullOrEmpty(nextScene) && nextScene != targetSceneID)
            {
                if (nextScene == "END") { yield return StartCoroutine(TransitionToSceneHendra()); yield break; }
                RunBranch(PlaySceneFromCSV(nextScene));
                yield break;
            }
        }
    }
    #endregion

    #region SCENE 01.01 CHOICES (DYNAMIC BUBBLE & GROUP PREFABS)
    public void OnSelectOptionA() { HideDialogBoxInstant(); RunBranch(RunOptionASequence()); }
    IEnumerator RunOptionASequence()
    {
        yield return StartCoroutine(PrepareInsideChatView("Pak Hendra", dosenProfileSprite));

        yield return StartCoroutine(SpawnChatBubble("Selamat pagi semua, mohon maaf perkuliahan hari ini saya undur menjadi jam 9", true));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(SpawnChatBubble("Baik Pak, terima kasih infonya.", false));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Fyuuh, untung aja kelas diundur jadi ga telat deh."));
        yield return StartCoroutine(WaitForTap());

        RunBranch(PlaySceneFromCSV("SCENE_01_02"));
    }

    public void OnSelectOptionB() { HideDialogBoxInstant(); RunBranch(RunOptionBSequence()); }
    IEnumerator RunOptionBSequence()
    {
        yield return StartCoroutine(PrepareInsideChatView("TMD 5B ASIK", grupProfileSprite));

        yield return StartCoroutine(SpawnChatBubble("Guys ini Pak Hendra meminta kita membuat kelompok untuk presentasi.", true, groupLeftBubblePrefab, taraProfileSprite));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(SpawnChatBubble("eh guys, siapa yang belum dapet kelompok ya?", true, groupLeftBubblePrefab, anggotaGrupSprite));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Duh apalagi sih baru juga masuk."));
        yield return StartCoroutine(WaitForTap());

        RunBranch(PlaySceneFromCSV("SCENE_01_02"));
    }

    public void OnSelectOptionC() { HideDialogBoxInstant(); RunBranch(RunOptionCSequence()); }
    IEnumerator RunOptionCSequence()
    {
        ClearChatContainer();
        if (choicePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(choicePanelCanvasGroup, 1f, 0f, 0.25f));
            SetChoicePanelActive(choicePanelCanvasGroup, false);
        }

        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(false);
        if (tablePhoneView != null) tablePhoneView.SetActive(true);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        if (blackOverlay != null) blackOverlay.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, 0.5f));

        if (interludeText != null) { interludeText.text = "Beberapa saat kemudian..."; interludeText.gameObject.SetActive(true); }
        yield return new WaitForSeconds(2.0f);
        if (interludeText != null) interludeText.gameObject.SetActive(false);

        if (tablePhoneView != null) tablePhoneView.SetActive(false);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(true);

        yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.5f));
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);

        PlaySFX(beepSFX);
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Ah udah banyak aja nih notifikasinya."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void ToggleDNDState()
    {
        HideDialogBoxInstant();
        isDndActive = true;
        if (dndToggleImage != null && dndOnSprite != null) dndToggleImage.sprite = dndOnSprite;
        if (dndStatusIcon != null) dndStatusIcon.SetActive(true);
        PlaySFX(beepSFX);
        RunBranch(TransitionToSceneHendra());
    }
    #endregion

    #region SCENE 01.02 - TARA CHAT
    IEnumerator RunScene0102TaraSequence()
    {
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
        yield return StartCoroutine(FadeOutDialogBoxSmooth(0.25f));

        if (phoneViewInsideChat != null && phoneViewInsideChat.activeSelf)
        {
            CanvasGroup phoneCg = phoneViewInsideChat.GetComponent<CanvasGroup>();
            if (phoneCg == null) phoneCg = phoneViewInsideChat.AddComponent<CanvasGroup>();

            yield return StartCoroutine(FadeCanvasGroup(phoneCg, 1f, 0f, 0.35f));
            phoneViewInsideChat.SetActive(false);
            yield return new WaitForSeconds(0.35f);
        }

        Sprite profileToUse = taraProfileSprite != null ? taraProfileSprite : dosenProfileSprite;
        yield return StartCoroutine(PrepareInsideChatView("Tara", profileToUse, fadeInPhone: true));

        yield return StartCoroutine(SpawnChatBubble("Eh, kamu masuk kelompok aku ya.", true));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(SpawnChatBubble("Presentasi minggu depan. Nanti aku kirim pembagian tugas.", true));
        yield return StartCoroutine(WaitForTap());

        HideDialogBoxInstant();
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
        SetChoicePanelActive(choicePanelCanvasGroup, false);
        SetChoicePanelActive(choicePanelTaraCanvasGroup, true);

        if (optionTaraCanvasGroups != null)
            foreach (CanvasGroup cg in optionTaraCanvasGroups) if (cg != null) { cg.gameObject.SetActive(true); cg.alpha = 0f; }

        yield return StartCoroutine(AnimateButtonsStaggered(optionTaraCanvasGroups));
    }

    public void OnSelectOption2A() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanelTaraCanvasGroup, false); RunBranch(RunOption2ASequence()); }
    IEnumerator RunOption2ASequence()
    {
        yield return StartCoroutine(SpawnChatBubble("iya, kirim aja.", false));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(SpawnChatBubble("Okedeh.", true));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOption2B() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanelTaraCanvasGroup, false); RunBranch(RunOption2BSequence()); }
    IEnumerator RunOption2BSequence()
    {
        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Jawab nanti aja deh."));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "'Nanti'. Satu kata yang sering terdengar seperti solusi, padahal kadang cuma cara menunda masalah."));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOption2C() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanelTaraCanvasGroup, false); RunBranch(RunOption2CSequence()); }
    IEnumerator RunOption2CSequence()
    {
        yield return StartCoroutine(SpawnChatBubble("Presentasinya kapan, Tara? Dan bagian tugas aku apa ya?", false));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(SpawnChatBubble("Senin depan ya, kerjain bagian pendahuluan aja.", true));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToSceneHendra());
    }
    #endregion

    #region SCENE 01.03 - PAK HENDRA (DIALOG LENGKAP + KEDIP 0.1s) & TRANSISI WEEK 2
    private void SetScene1_3Setup()
    {
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(false);
        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);

        if (mainBackgroundImage != null && bgRuangKelas != null)
        {
            mainBackgroundImage.gameObject.SetActive(true);
            mainBackgroundImage.sprite = bgRuangKelas;
        }
        if (hendraCharacterImage != null && hendraNormalSprite != null)
        {
            hendraCharacterImage.gameObject.SetActive(true);
            hendraCharacterImage.sprite = hendraNormalSprite;
            hendraCharacterImage.SetNativeSize();
        }
        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
    }

    IEnumerator RunScene0103HendraSequence()
    {
        SetScene1_3Setup();

        // 3 Dialog Pak Hendra dijalankan utuh satu per satu tanpa kepotong
        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Sebelum mulai, Saya ingin mengingatkan satu hal."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Belakangan ada informasi yang beredar mengenai soal UTS yang sering dibocorkan."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Sebenarnya itu terserah kalian, toh kalian sudah dewasa. Harus pintar-pintar dalam memilah informasi."));
        yield return StartCoroutine(WaitForTap());

        HideDialogBoxInstant();
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        SetChoicePanelActive(choicePanel1_3CanvasGroup, true);
        if (option1_3CanvasGroups != null)
            foreach (CanvasGroup cg in option1_3CanvasGroups) if (cg != null) { cg.gameObject.SetActive(true); cg.alpha = 0f; }

        yield return StartCoroutine(AnimateButtonsStaggered(option1_3CanvasGroups));
    }

    public void OnSelectOption3A() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanel1_3CanvasGroup, false); RunBranch(RunOption3ASequence()); }
    IEnumerator RunOption3ASequence()
    {
        // Transisi kedip cepat 0.1s sebelum muncul visual aksi Kirana
        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => ShowActionOverlay(spriteMcCatat)));

        yield return new WaitForSeconds(1.5f);

        // Kedip cepat 0.1s balik ke pose Pak Hendra
        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => HideActionOverlayAndSetHendra(hendraPraiseSprite)));

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Bagus Kirana, lanjutkan"));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToWeek2());
    }

    public void OnSelectOption3B() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanel1_3CanvasGroup, false); RunBranch(RunOption3BSequence()); }
    IEnumerator RunOption3BSequence()
    {
        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => ShowActionOverlay(spriteMcLihatHp)));

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => HideActionOverlayAndSetHendra(hendraSilentStareSprite)));

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "(Pak Hendra berhenti sebentar, menatap Kirana tanpa berkata apa-apa)"));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToWeek2());
    }

    public void OnSelectOption3C() { HideDialogBoxInstant(); SetChoicePanelActive(choicePanel1_3CanvasGroup, false); RunBranch(RunOption3CSequence()); }
    IEnumerator RunOption3CSequence()
    {
        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => ShowActionOverlay(spriteMcAngkatTangan)));

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Maaf pak, izin bertanya. Kalau kita mendapatkan informasi seperti itu, mending diam saja atau bagaimana ya pak?"));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(QuickBlinkTransition(0.1f, () => HideActionOverlayAndSetHendra(hendraTegasSprite)));

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Tanya sumber pertama. Jangan ikut menyebarkan sesuatu yang bahkan belum kamu pahami."));
        yield return StartCoroutine(WaitForTap());
        yield return StartCoroutine(TransitionToWeek2());
    }

    private IEnumerator TransitionToWeek2()
    {
        yield return StartCoroutine(FadeOutDialogBoxSmooth(0.3f));
        if (hendraCharacterImage != null) hendraCharacterImage.gameObject.SetActive(false);
        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
        if (week1Section != null) week1Section.SetActive(false);
        if (statusPanel != null) statusPanel.SetActive(false);

        if (week2Manager != null)
        {
            week2Manager.gameObject.SetActive(true);
            week2Manager.StartWeek2();
        }
    }
    #endregion

    #region DYNAMIC CHAT & DIALOGUE HELPERS
    IEnumerator SpawnChatBubble(string message, bool isLeft, GameObject customPrefab = null, Sprite avatar = null)
    {
        EnsureTapButtonActive();
        GameObject prefab = customPrefab != null ? customPrefab : (isLeft ? leftBubblePrefab : rightBubblePrefab);
        if (chatContainer == null || prefab == null) yield break;

        GameObject bubble = Instantiate(prefab, chatContainer, false);

        if (customPrefab != null && avatar != null)
        {
            Transform avatarHolder = bubble.transform.Find("HeaderPanel (1)");
            if (avatarHolder != null)
            {
                Image[] imgs = avatarHolder.GetComponentsInChildren<Image>(true);
                if (imgs.Length > 0) imgs[imgs.Length - 1].sprite = avatar;
            }
        }

        TMP_Text bubbleText = bubble.GetComponentInChildren<TMP_Text>();
        if (bubbleText != null)
        {
            fullTextHolder = message;
            isTyping = true;
            bubbleText.text = "";

            foreach (char letter in message)
            {
                if (!isTyping) break;
                bubbleText.text += letter;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainer.GetComponent<RectTransform>());
                yield return new WaitForSeconds(0.035f);
            }

            bubbleText.text = fullTextHolder;
            isTyping = false;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainer.GetComponent<RectTransform>());
        PlaySFX(messageSFX);
    }

    private void ClearChatContainer()
    {
        if (chatContainer == null) return;
        foreach (Transform child in chatContainer) Destroy(child.gameObject);
    }

    IEnumerator PrepareInsideChatView(string headerTitle, Sprite profileSprite, bool fadeInPhone = false)
    {
        ClearChatContainer();
        if (choicePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(choicePanelCanvasGroup, choicePanelCanvasGroup.alpha, 0f, 0.25f));
            SetChoicePanelActive(choicePanelCanvasGroup, false);
        }

        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        if (headerNameText != null) headerNameText.text = headerTitle;
        if (headerProfileImage != null && profileSprite != null) headerProfileImage.sprite = profileSprite;

        if (phoneViewInsideChat != null)
        {
            CanvasGroup phoneCg = phoneViewInsideChat.GetComponent<CanvasGroup>();
            if (phoneCg == null) phoneCg = phoneViewInsideChat.AddComponent<CanvasGroup>();

            phoneViewInsideChat.SetActive(true);
            if (fadeInPhone)
            {
                phoneCg.alpha = 0f;
                if (insidePhoneTransform != null) StartCoroutine(AnimatePhoneScale(insidePhoneTransform));
                yield return StartCoroutine(FadeCanvasGroup(phoneCg, 0f, 1f, 0.4f));
            }
            else
            {
                phoneCg.alpha = 1f;
                if (insidePhoneTransform != null) StartCoroutine(AnimatePhoneScale(insidePhoneTransform));
            }
        }
    }

    private IEnumerator TransitionToSceneHendra()
    {
        yield return StartCoroutine(FadeOutDialogBoxSmooth(0.3f));
        if (interludeText != null) interludeText.gameObject.SetActive(false);

        if (blackOverlay != null)
        {
            blackOverlay.transform.SetAsLastSibling();
            blackOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, 0.4f));
        }

        if (tablePhoneView != null) tablePhoneView.SetActive(false);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);
        if (phoneViewInsideChat != null)
        {
            CanvasGroup phoneCg = phoneViewInsideChat.GetComponent<CanvasGroup>();
            if (phoneCg != null) phoneCg.alpha = 1f;
            phoneViewInsideChat.SetActive(false);
        }
        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        ClearChatContainer();
        SetChoicePanelActive(choicePanelTaraCanvasGroup, false);

        SetScene1_3Setup();
        yield return new WaitForSeconds(0.15f);

        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.4f));
            blackOverlay.gameObject.SetActive(false);
        }

        // Langsung mulai sequence Pak Hendra setelah layar terbuka penuh
        RunBranch(RunScene0103HendraSequence());
    }

    // Helper kedip cepat 0.1 detik (fade out 0.1s -> ganti visual -> fade in 0.1s)
    private IEnumerator QuickBlinkTransition(float halfDuration, System.Action onMidBlink)
    {
        if (blackOverlay != null)
        {
            blackOverlay.transform.SetAsLastSibling();
            blackOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, halfDuration));
            onMidBlink?.Invoke();
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, halfDuration));
            blackOverlay.gameObject.SetActive(false);
        }
        else
        {
            onMidBlink?.Invoke();
        }
    }

    IEnumerator ShowDialogueWithTyping(string speaker, string text)
    {
        EnsureTapButtonActive();

        if (dialogBoxBackgroundImage != null)
        {
            string s = speaker != null ? speaker.Trim().ToUpper() : "";
            if ((s.Contains("HENDRA") || s.Contains("PAK")) && dialogBoxHendraSprite != null) dialogBoxBackgroundImage.sprite = dialogBoxHendraSprite;
            else if ((s.Contains("SAGARA") || s.Contains("TARA")) && dialogBoxTaraSprite != null) dialogBoxBackgroundImage.sprite = dialogBoxTaraSprite;
            else if (dialogBoxKiranaSprite != null) dialogBoxBackgroundImage.sprite = dialogBoxKiranaSprite;
        }

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                if (cg.alpha < 0.99f)
                {
                    cg.alpha = 0f;
                    if (characterNameText != null) characterNameText.text = speaker;
                    if (dialogueText != null) dialogueText.text = "";
                    yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, dialogueFadeInDuration));
                }
                else if (characterNameText != null) characterNameText.text = speaker;
            }
        }

        fullTextHolder = text;
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char letter in text)
        {
            if (!isTyping) break;
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (dialogueText != null) dialogueText.text = fullTextHolder;
        isTyping = false;
    }

    private void ShowActionOverlay(Sprite s) { if (actionOverlayImage != null && s != null) { actionOverlayImage.sprite = s; actionOverlayImage.gameObject.SetActive(true); } }
    private void HideActionOverlayAndSetHendra(Sprite pose)
    {
        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null && pose != null) { hendraCharacterImage.sprite = pose; hendraCharacterImage.SetNativeSize(); }
    }

    private void HideDialogBoxInstant()
    {
        if (dialogPanel == null) return;
        CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
        if (cg != null) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
        dialogPanel.SetActive(false);
    }

    private IEnumerator FadeOutDialogBoxSmooth(float duration)
    {
        if (dialogPanel == null || !dialogPanel.activeSelf) yield break;
        CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
        if (cg != null) { yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, duration)); cg.interactable = false; cg.blocksRaycasts = false; }
        dialogPanel.SetActive(false);
    }

    public void OnUserTap() { if (isTyping) isTyping = false; else userTapped = true; }
    IEnumerator WaitForTap() { userTapped = false; while (!userTapped) yield return null; userTapped = false; }

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null) { invisibleTapButton.gameObject.SetActive(true); invisibleTapButton.transform.SetAsLastSibling(); }
    }

    public void ResetToMainList()
    {
        if (week1Section != null) week1Section.SetActive(true);
        if (phoneMainListChat != null) phoneMainListChat.SetActive(true);
        if (phoneViewInsideChat != null)
        {
            CanvasGroup phoneCg = phoneViewInsideChat.GetComponent<CanvasGroup>();
            if (phoneCg != null) phoneCg.alpha = 1f;
            phoneViewInsideChat.SetActive(false);
        }
        if (phoneViewScreenOff != null) phoneViewScreenOff.SetActive(false);
        if (tablePhoneView != null) tablePhoneView.SetActive(false);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);
        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null) hendraCharacterImage.gameObject.SetActive(false);
        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
        HideDialogBoxInstant();

        isDndActive = false;
        if (dndToggleImage != null && dndOffSprite != null) dndToggleImage.sprite = dndOffSprite;

        ClearChatContainer();
        SetChoicePanelActive(choicePanelCanvasGroup, true);
        SetChoicePanelActive(choicePanelTaraCanvasGroup, false);
        SetChoicePanelActive(choicePanel1_3CanvasGroup, false);

        if (optionCanvasGroups != null)
            foreach (CanvasGroup cg in optionCanvasGroups) if (cg != null) { cg.gameObject.SetActive(true); cg.interactable = false; cg.blocksRaycasts = false; cg.alpha = 0f; }
    }

    IEnumerator AnimateButtonsStaggered(CanvasGroup[] targetButtons)
    {
        yield return new WaitForSeconds(delayBeforeFirstButton);
        if (targetButtons == null) yield break;
        foreach (CanvasGroup cg in targetButtons)
        {
            if (cg != null)
            {
                cg.gameObject.SetActive(true);
                yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, buttonFadeDuration));
                cg.interactable = true; cg.blocksRaycasts = true;
                yield return new WaitForSeconds(delayBetweenButtons);
            }
        }
    }

    IEnumerator AnimatePhoneScale(Transform target)
    {
        Vector3 start = new Vector3(0.95f, 0.95f, 1f);
        target.localScale = start;
        for (float t = 0f; t < phoneZoomDuration; t += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(start, Vector3.one, t / phoneZoomDuration);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float dur)
    {
        if (cg == null) yield break;
        for (float t = 0f; t < dur; t += Time.deltaTime) { cg.alpha = Mathf.Lerp(start, end, t / dur); yield return null; }
        cg.alpha = end;
    }

    private IEnumerator FadeImageAlpha(Image img, float start, float end, float dur)
    {
        if (img == null) yield break;
        for (float t = 0f; t < dur; t += Time.deltaTime) { img.color = new Color(img.color.r, img.color.g, img.color.b, Mathf.Lerp(start, end, t / dur)); yield return null; }
        img.color = new Color(img.color.r, img.color.g, img.color.b, end);
    }

    private void SetChoicePanelActive(CanvasGroup cg, bool state)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(state);
        cg.alpha = state ? 1f : 0f;
        cg.interactable = state;
        cg.blocksRaycasts = state;
    }

    private void PlaySFX(AudioClip clip) { if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip); }
    #endregion
}