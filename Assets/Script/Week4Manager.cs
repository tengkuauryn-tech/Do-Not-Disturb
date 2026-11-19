using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class Week4Manager : MonoBehaviour
{
    [Header("Main Containers")]
    public GameObject week4Section; 

    [Header("Visual Layers (Urutan Adegan Week 4)")]
    public GameObject bgPortalNilai; 
    public GameObject phoneUI;       

    [Header("Phone UI Elements (Header HP)")]
    public TMP_Text contactNameText;          
    public Image profilePicImage;             
    public Sprite sagaraProfileSprite;        

    [Header("Chat Spawner (Prefabs & Container)")]
    public Transform chatContainer;           
    public GameObject leftBubblePrefab;      
    public GameObject rightBubblePrefab;     

    [Header("Dialogue System (GLOBAL UI)")]
    public GameObject dialogBoxObject;     
    public TMP_Text dialogueText;          
    public Button invisibleTapButton;      
    public Sprite dialogBoxKiranaSprite;   

    [Header("Choice Panel & Buttons")]
    public CanvasGroup choicePanelWeek4;       
    public CanvasGroup[] optionCanvasGroups;   
    public Button buttonAText;                 
    public Button buttonBText;                 
    public Button buttonRest;                  // <-- Slot baru untuk tombol Rest / Istirahat
    public float delayBeforeFirstButton = 0.2f;
    public float delayBetweenButtons = 0.15f;
    public float buttonFadeDuration = 0.3f;

    [Header("Interlude / Overlay Assets")]
    public Image blackOverlay;               
    public TMP_Text weekTitleText;           

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip messageSFX;
    public AudioClip beepSFX;

    // ==========================================
    // SLOT CHOICE DATA
    // ==========================================
    [Header("Stat Events (Isi dengan StatManager & ChoiceData)")]
    public UnityEvent onApplyCloseA;
    public UnityEvent onApplyCloseB;
    public UnityEvent onApplyDistantA;
    public UnityEvent onApplyDistantB;
    public UnityEvent onApplyRest;             // <-- Event pengubah poin saat pilih Istirahat
    // ==========================================

    private Image dialogBoxImageComponent;
    private bool userTapped = false;
    private bool isTyping = false;
    private string fullChatTextHolder = "";

    void Awake()
    {
        if (dialogBoxObject != null)
        {
            dialogBoxImageComponent = dialogBoxObject.GetComponent<Image>();
        }
        ResetChoicePanel();
    }

    public void StartWeek4()
    {
        if (week4Section != null) week4Section.SetActive(true);

        Week3Manager w3 = FindFirstObjectByType<Week3Manager>(FindObjectsInactive.Include);
        if (w3 != null) 
        { 
            if (w3.week3Section != null) w3.week3Section.SetActive(false);
            w3.StopAllCoroutines(); 
            w3.enabled = false; 
        }

        if (dialogueText != null) dialogueText.text = "";

        EnsureTapButtonActive();
        if (invisibleTapButton != null)
        {
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        if (phoneUI) 
        {
            phoneUI.SetActive(false);
            CanvasGroup cg = phoneUI.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f; 
        }
        
        if (dialogBoxObject) dialogBoxObject.SetActive(false);
        ResetChoicePanel();

        // Setup Overlay Hitam Awal (Fade-in Judul)
        if (blackOverlay != null) 
        {
            blackOverlay.gameObject.SetActive(true);
            Color c = blackOverlay.color;
            c.a = 1f;
            blackOverlay.color = c;
        }

        if (weekTitleText != null)
        {
            weekTitleText.text = "Week 4 - Mid-Point Evaluation";
            weekTitleText.gameObject.SetActive(true);
            Color textColor = weekTitleText.color;
            textColor.a = 1f;
            weekTitleText.color = textColor;
        }

        if (bgPortalNilai) bgPortalNilai.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(RunWeek4Sequence());
    }

    IEnumerator RunWeek4Sequence()
    {
        userTapped = false;
        isTyping = false;

        yield return new WaitForSeconds(1.8f);

        if (weekTitleText != null) weekTitleText.gameObject.SetActive(false);

        // Fade Out Layar Hitam Judul
        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.8f));
            blackOverlay.gameObject.SetActive(false); 
        }

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(PlayDialog("Kirana", "Aduh, malu-maluin banget deh liat nilai jelek di web."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false); 
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.6f);

        SetPhoneHeader("Sagara", sagaraProfileSprite);
        
        if (chatContainer != null)
        {
            foreach (Transform child in chatContainer) Destroy(child.gameObject);
        }

        if (phoneUI != null) 
        {
            yield return StartCoroutine(FadeInUI(phoneUI, 0.7f));
        }
        
        yield return new WaitForSeconds(0.3f); 

        if (Week2Manager.isPathDistant)
        {
            yield return StartCoroutine(SpawnAndTypeChat("Permisi Kirana, maaf ganggu malem-malem.", false));
            PlaySFX(messageSFX);

            yield return StartCoroutine(SpawnAndTypeChat("Ini tadi Pak Hendra minta rekap nilai kuis kelompok, punya kamu udah di-submit kan?", false));
            PlaySFX(messageSFX);

            if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
            ShowChoicesDistant();
        }
        else
        {
            yield return StartCoroutine(SpawnAndTypeChat("Kamu belum tidur?", false));
            PlaySFX(messageSFX);

            yield return StartCoroutine(SpawnAndTypeChat("Aku tadi liat nilai kamu, kamu gapapa?", false));
            PlaySFX(messageSFX);

            if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
            ShowChoicesClose();
        }
    }

    // ==========================================
    // PILIHAN JALUR CLOSE, DISTANT & REST
    // ==========================================
    private void SetupRestButton()
    {
        if (buttonRest != null)
        {
            buttonRest.onClick.RemoveAllListeners();
            buttonRest.onClick.AddListener(OnSelectRest);
        }
    }

    private void ShowChoicesClose()
    {
        SetButtonText(buttonAText, "A. Ungkapkan kecemasan");
        SetButtonText(buttonBText, "B. Alihkan pembicaraan");

        buttonAText.onClick.RemoveAllListeners();
        buttonAText.onClick.AddListener(OnSelectCloseOptionA);

        buttonBText.onClick.RemoveAllListeners();
        buttonBText.onClick.AddListener(OnSelectCloseOptionB);

        SetupRestButton();

        if (choicePanelWeek4 != null) choicePanelWidthAndShow();
    }

    public void OnSelectCloseOptionA() 
    {
        onApplyCloseA?.Invoke();
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunCloseOptionASequence());
    }

    IEnumerator RunCloseOptionASequence()
    {
        yield return StartCoroutine(SpawnAndTypeChat("Ya aku sedih sih... padahal aku udah begadang buat belajar, tapi hasilnya tetep ngecewain.", true));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SpawnAndTypeChat("Nanti kita ketemu aja yuk.", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(TransitionToWeek5());
    }

    public void OnSelectCloseOptionB() 
    {
        onApplyCloseB?.Invoke();
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunCloseOptionBSequence());
    }

    IEnumerator RunCloseOptionBSequence()
    {
        yield return StartCoroutine(SpawnAndTypeChat("Aman kok, masih bisa dikejar di UTS nanti.", true));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SpawnAndTypeChat("Oke kalau gitu. Besok jadi ya kita belajar bareng", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SpawnAndTypeChat("Jangan lupa istirahat Kirana", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(TransitionToWeek5());
    }

    private void ShowChoicesDistant()
    {
        SetButtonText(buttonAText, "A. Jawab singkat & formal");
        SetButtonText(buttonBText, "B. Read saja");

        buttonAText.onClick.RemoveAllListeners();
        buttonAText.onClick.AddListener(OnSelectDistantOptionA);

        buttonBText.onClick.RemoveAllListeners();
        buttonBText.onClick.AddListener(OnSelectDistantOptionB);

        SetupRestButton();

        if (choicePanelWeek4 != null) choicePanelWidthAndShow();
    }

    public void OnSelectDistantOptionA() 
    {
        onApplyDistantA?.Invoke();
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunDistantOptionASequence());
    }

    IEnumerator RunDistantOptionASequence()
    {
        yield return StartCoroutine(SpawnAndTypeChat("Udah kok di portal.", true));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SpawnAndTypeChat("Oke makasih infonya. Maaf ganggu.", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(TransitionToWeek5());
    }

    public void OnSelectDistantOptionB() 
    {
        onApplyDistantB?.Invoke();
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunDistantOptionBSequence());
    }

    IEnumerator RunDistantOptionBSequence()
    {
        if (phoneUI != null) phoneUI.SetActive(false);

        yield return StartCoroutine(PlayDialog("Kirana", "Aku mematikan layar HP dan menatap langit-langit kamar."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TransitionToWeek5());
    }

    // --- HANDLER PILIHAN REST / ISTIRAHAT ---
    public void OnSelectRest()
    {
        onApplyRest?.Invoke();
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunRestSequence());
    }

    IEnumerator RunRestSequence()
    {
        if (phoneUI != null) phoneUI.SetActive(false);
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // Fade ke layar hitam
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.transform.SetAsLastSibling();
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, blackOverlay.color.a, 1f, 0.7f));
        }

        // Tampilkan teks interlude istirahat singkat
        if (weekTitleText != null)
        {
            weekTitleText.text = "Kirana memilih untuk beristirahat malam ini...";
            weekTitleText.transform.SetAsLastSibling();
            weekTitleText.gameObject.SetActive(true);
            Color textColor = weekTitleText.color;
            textColor.a = 1f;
            weekTitleText.color = textColor;
        }

        yield return new WaitForSeconds(2.0f);

        if (weekTitleText != null) weekTitleText.gameObject.SetActive(false);

        // Langsung masuk ke Week 5
        yield return StartCoroutine(TransitionToWeek5());
    }

    // ==========================================
    // TRANSISI KE WEEK 5
    // ==========================================
    IEnumerator TransitionToWeek5()
    {
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.transform.SetAsLastSibling();
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, blackOverlay.color.a, 1f, 0.8f));
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
        }

        Week5Manager w5 = FindFirstObjectByType<Week5Manager>(FindObjectsInactive.Include);
        if (w5 != null)
        {
            w5.gameObject.SetActive(true);
            w5.enabled = true;
            w5.StartWeek5();
        }

        if (week4Section != null)
        {
            week4Section.SetActive(false);
        }

        this.enabled = false;
    }

    // ==========================================
    // HELPER FUNCTIONS (CHAT & UI)
    // ==========================================
    private void SetButtonText(Button btn, string textStr)
    {
        if (btn != null)
        {
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = textStr;
        }
    }

    IEnumerator SpawnAndTypeChat(string message, bool isFromPlayer)
    {
        if (chatContainer == null) yield break;
        
        GameObject prefab = isFromPlayer ? rightBubblePrefab : leftBubblePrefab;
        if (prefab != null)
        {
            GameObject bubble = Instantiate(prefab, chatContainer, false); 
            TMP_Text textComp = bubble.GetComponentInChildren<TMP_Text>();
            
            if (textComp != null)
            {
                fullChatTextHolder = message;
                textComp.text = ""; 
                isTyping = true;
                EnsureTapButtonActive();

                RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
                RectTransform containerRect = chatContainer.GetComponent<RectTransform>();

                foreach (char c in message)
                {
                    if (!isTyping) break; 
                    textComp.text += c;
                    
                    Canvas.ForceUpdateCanvases();
                    if (bubbleRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
                    if (containerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

                    yield return new WaitForSeconds(0.03f); 
                }

                textComp.text = fullChatTextHolder;
                isTyping = false;

                Canvas.ForceUpdateCanvases();
                if (bubbleRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
                if (containerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }
        }
        yield return new WaitForSeconds(0.3f);
    }

    private void choicePanelWidthAndShow()
    {
        if (choicePanelWeek4 != null)
        {
            choicePanelWeek4.gameObject.SetActive(true);
            choicePanelWeek4.alpha = 1f;
            choicePanelWeek4.interactable = true;
            choicePanelWeek4.blocksRaycasts = true;
        }

        if (optionCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionCanvasGroups)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                }
            }
            StartCoroutine(AnimateButtonsStaggered(optionCanvasGroups));
        }
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

    private void ResetChoicePanel()
    {
        if (choicePanelWeek4 != null)
        {
            choicePanelWeek4.alpha = 0f; 
            choicePanelWeek4.gameObject.SetActive(false);
            choicePanelWeek4.interactable = false;
            choicePanelWeek4.blocksRaycasts = false;
        }

        if (optionCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.interactable = false;
                    cg.blocksRaycasts = false;
                    cg.gameObject.SetActive(false);
                }
            }
        }
    }

    public void SetPhoneHeader(string name, Sprite avatarSprite)
    {
        if (contactNameText != null) contactNameText.text = name;
        if (profilePicImage != null && avatarSprite != null)
        {
            profilePicImage.sprite = avatarSprite;
            profilePicImage.gameObject.SetActive(true);
        }
    }

    IEnumerator PlayDialog(string speaker, string text)
    {
        EnsureTapButtonActive();

        if (dialogueText != null) dialogueText.text = "";

        if (dialogBoxObject != null) 
        {
            if (dialogBoxImageComponent != null && dialogBoxKiranaSprite != null)
            {
                dialogBoxImageComponent.sprite = dialogBoxKiranaSprite;
            }

            dialogBoxObject.transform.SetAsLastSibling();
            dialogBoxObject.SetActive(true);

            CanvasGroup cg = dialogBoxObject.GetComponent<CanvasGroup>();
            if (cg != null) 
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        userTapped = false;
        yield return new WaitForSeconds(0.2f);
        userTapped = false;

        isTyping = true;
        
        foreach (char c in text)
        {
            if (!isTyping) break; 
            if (dialogueText != null) dialogueText.text += c;
            
            yield return new WaitForSeconds(0.04f);
        }

        if (dialogueText != null) dialogueText.text = text;
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

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTrialBringToFront();
        }
    }

    private void invisibleTrialBringToFront()
    {
        if (invisibleTapButton != null) invisibleTapButton.transform.SetAsLastSibling();
    }

    IEnumerator FadeInUI(GameObject target, float duration)
    {
        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        target.SetActive(true);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration); 
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeImageAlpha(Image img, float startAlpha, float endAlpha, float duration)
    {
        if (img == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
            yield return null;
        }
        img.color = new Color(img.color.r, img.color.g, img.color.b, endAlpha);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }
}