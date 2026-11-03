using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Week3Manager : MonoBehaviour
{
    [Header("Main Containers")]
    public GameObject week3Section; 

    [Header("Visual Layers (Urutan Adegan)")]
    public GameObject bgKamar;      
    public GameObject bgLaptop;     
    public GameObject bgNotifHP;    
    public GameObject phoneUI;      

    [Header("Phone UI Elements (Header HP)")]
    public TMP_Text contactNameText;          
    public Image profilePicImage;             
    public Sprite taraProfileSprite;          
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

    [Header("Choice Panel & Buttons (Array 4 Elemen)")]
    public CanvasGroup choicePanelWeek3;       
    public CanvasGroup[] optionCanvasGroups;   // Size: 4 (0: DND, 1: Tombol A, 2: Tombol B, 3: Tombol C)

    [Header("Choice 3.01 Conditional Locks (Sagara Path)")]
    public Button optionBSagaraButton;         
    public CanvasGroup optionBCanvasGroup;     

    [Header("DND Mechanic Controls (Sprite Swap)")]
    public Button dndToggleButton;
    public Image dndToggleImage;          
    public Sprite dndOffSprite;           
    public Sprite dndOnSprite;            
    public GameObject dndStatusIcon;      
    [HideInInspector] public bool isDndActive = false;

    [Header("Option C & Interlude Assets")]
    public Image blackOverlay;            

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip messageSFX;
    public AudioClip beepSFX;

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

    public void StartWeek3()
    {
        if (week3Section != null) week3Section.SetActive(true);

        Week1Manager w1 = FindFirstObjectByType<Week1Manager>(FindObjectsInactive.Include);
        if (w1 != null) { w1.StopAllCoroutines(); w1.enabled = false; }

        Week2Manager w2 = FindFirstObjectByType<Week2Manager>(FindObjectsInactive.Include);
        if (w2 != null) { w2.StopAllCoroutines(); w2.enabled = false; }

        if (dialogueText != null) dialogueText.text = "";

        EnsureTapButtonActive();
        if (invisibleTapButton != null)
        {
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        // Setup DND Awal (Mati)
        isDndActive = false;
        if (dndToggleImage != null && dndOffSprite != null) 
        {
            dndToggleImage.sprite = dndOffSprite;
            Color c = dndToggleImage.color;
            c.a = 0.4f; 
            dndToggleImage.color = c;
        }
        if (dndStatusIcon != null) dndStatusIcon.SetActive(false);
        if (blackOverlay != null) 
        {
            blackOverlay.gameObject.SetActive(false);
            blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, 0f);
        }

        if (bgKamar) bgKamar.SetActive(true);
        if (bgLaptop) bgLaptop.SetActive(false);
        if (bgNotifHP) bgNotifHP.SetActive(false);
        
        if (phoneUI) 
        {
            phoneUI.SetActive(false);
            CanvasGroup cg = phoneUI.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f; 
        }
        
        if (dialogBoxObject) dialogBoxObject.SetActive(false);
        ResetChoicePanel();

        StopAllCoroutines();
        StartCoroutine(RunWeek3Sequence());
    }

    IEnumerator RunWeek3Sequence()
    {
        userTapped = false;
        isTyping = false;

        // --- TAHAP 1: KAMAR KOS & DIALOG AWAL ---
        if (bgKamar) bgKamar.SetActive(true);
        yield return new WaitForSeconds(1.5f); 
        
        yield return StartCoroutine(PlayDialog("Kirana", "Udah tiga minggu aja ya kuliah."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false); 
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.6f); 

        // --- TAHAP 2: PINDAH KE LAPTOP & SAMBUNGAN DIALOG ---
        if (bgKamar) bgKamar.SetActive(false);
        if (bgLaptop) bgLaptop.SetActive(true);
        
        yield return new WaitForSeconds(1.2f);
        yield return StartCoroutine(PlayDialog("Kirana", "Ga kerasa..."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false); 
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(1.0f); 

        // --- TAHAP 3: LAYAR NOTIF GETAR & DIALOG NGEDUMEL ---
        if (bgLaptop) bgLaptop.SetActive(false);
        if (bgNotifHP) 
        {
            bgNotifHP.SetActive(true);
            yield return StartCoroutine(ShakeEffect(bgNotifHP, 0.4f, 15f)); 
        }
        
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(PlayDialog("Kirana", "Berisik banget.. Ada notifikasi apalagi sih?"));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false); 
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);

        // --- TAHAP 4: BUKA HP & CHAT TARA ---
        if (bgNotifHP) bgNotifHP.SetActive(false);
        if (bgLaptop) bgLaptop.SetActive(true); 

        SetPhoneHeader("Tara", taraProfileSprite);

        if (phoneUI) 
        {
            yield return StartCoroutine(FadeInUI(phoneUI, 0.7f));
        }
        
        yield return new WaitForSeconds(0.8f);

        if (chatContainer != null)
        {
            foreach (Transform child in chatContainer)
            {
                Destroy(child.gameObject);
            }
        }

       yield return StartCoroutine(SpawnAndTypeChat("Kir bagian makalah bab 2 bisa bantu selesain ga buat malem ini?", false));

        // MATIKAN TOMBOL TAP INVISIBLE AGAR TIDAK MENGHALANGI KLIK PILIHAN
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        // --- TAHAP 5: MUNCULKAN PANEL PILIHAN INSTAN ---
        CheckConditionalChoices();
        ShowAllChoicesInstant();
    }

    private void CheckConditionalChoices()
    {
        if (Week2Manager.isPathDistant)
        {
            if (optionBSagaraButton != null) optionBSagaraButton.interactable = false; 
            if (optionBCanvasGroup != null) optionBCanvasGroup.alpha = 0.35f; 
        }
        else
        {
            if (optionBSagaraButton != null) optionBSagaraButton.interactable = true;
            if (optionBCanvasGroup != null) optionBCanvasGroup.alpha = 1f;
        }
    }

    // ==========================================
    // BRANCHING CHOICES
    // ==========================================

    public void OnSelectOption3A()
    {
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunOptionASequence());
    }

    IEnumerator RunOptionASequence()
    {
        yield return StartCoroutine(SpawnAndTypeChat("Oke nanti aku bantuin tar.", true));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.5f);

        SetPhoneHeader("Tara", taraProfileSprite);
        yield return StartCoroutine(SpawnAndTypeChat("MAKASIHHHH YAA", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(1.0f);
    }

    public void OnSelectOption3B()
    {
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunOptionBSequence());
    }

    IEnumerator RunOptionBSequence()
    {
        SetPhoneHeader("Sagara", sagaraProfileSprite);
        if (chatContainer != null)
        {
            foreach (Transform child in chatContainer)
                Destroy(child.gameObject);
        }

        yield return StartCoroutine(SpawnAndTypeChat("Haloo sagara, kamu masih online? Umm bisa bantuin tugas kelompok gaa?", true));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(SpawnAndTypeChat("Amann kok, aku bisa bantuin.", false));
        PlaySFX(messageSFX);
        yield return new WaitForSeconds(1.0f);
    }

    public void OnSelectOption3C()
    {
        ResetChoicePanel();
        StopAllCoroutines();
        StartCoroutine(RunOptionCSequence());
    }

    IEnumerator RunOptionCSequence()
    {
        if (phoneUI != null) phoneUI.SetActive(false);
        if (bgLaptop != null) bgLaptop.SetActive(false);
        if (bgKamar != null) bgKamar.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(PlayDialog("Kirana", "Besok aja lah."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, 0.8f));
        }
    }

    public void ToggleDNDState()
    {
        HideDialogBoxInstant();
        isDndActive = true;

        if (dndToggleImage != null && dndOnSprite != null) 
        {
            dndToggleImage.sprite = dndOnSprite; 
            Color c = dndToggleImage.color;
            c.a = 1f; 
            dndToggleImage.color = c;
        }
        if (dndStatusIcon != null) dndStatusIcon.SetActive(true);

        PlaySFX(beepSFX);
        
        StopAllCoroutines();
        StartCoroutine(RunDNDSequence());
    }

    IEnumerator RunDNDSequence()
    {
        // Jalankan FadeOut bersamaan untuk Panel Pilihan (Blur Overlay) dan PhoneUI
        Coroutine fadeChoice = null;
        Coroutine fadePhone = null;

        if (choicePanelWeek3 != null)
            fadeChoice = StartCoroutine(FadeOutUI(choicePanelWeek3.gameObject, 0.5f));

        if (phoneUI != null)
            fadePhone = StartCoroutine(FadeOutUI(phoneUI, 0.5f));

        // Tunggu hingga proses fade out selesai
        if (fadeChoice != null) yield return fadeChoice;
        if (fadePhone != null) yield return fadePhone;

        ResetChoicePanel();

        if (bgNotifHP != null) bgNotifHP.SetActive(false);
        if (bgKamar != null) bgKamar.SetActive(false);
        if (bgLaptop != null) bgLaptop.SetActive(true);

        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(PlayDialog("Kirana", "Aduh berisik, orang lagi belajar juga..."));
        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
    }

    // ==========================================
    // HELPER FUNCTIONS
    // ==========================================

    private void HideDialogBoxInstant()
    {
        if (dialogBoxObject != null)
        {
            CanvasGroup cg = dialogBoxObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            dialogBoxObject.SetActive(false);
        }
    }

    IEnumerator SpawnAndTypeChat(string message, bool isFromPlayer)
    {
        if (chatContainer == null) yield break;
        
        GameObject prefab = isFromPlayer ? rightBubblePrefab : leftBubblePrefab;
        if (prefab != null)
        {
            // Spawn bubble ke dalam chat container
            GameObject bubble = Instantiate(prefab, chatContainer, false); 
            TMP_Text textComp = bubble.GetComponentInChildren<TMP_Text>();
            
            if (textComp != null)
            {
                fullChatTextHolder = message;
                textComp.text = ""; 
                isTyping = true;
                EnsureTapButtonActive();

                // Ambil RectTransform untuk di-update paksa
                RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
                RectTransform containerRect = chatContainer.GetComponent<RectTransform>();

                foreach (char c in message)
                {
                    if (!isTyping) break; 
                    textComp.text += c;
                    
                    // --- KUNCI SOLUSINYA DI SINI ---
                    // Paksa Unity update ukuran kotak bubble DAN dorong posisi di container
                    // secara real-time SETIAP KALI SATU HURUF MUNCUL
                    Canvas.ForceUpdateCanvases();
                    if (bubbleRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
                    if (containerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

                    yield return new WaitForSeconds(0.03f); 
                }

                textComp.text = fullChatTextHolder;
                isTyping = false;

                // Pastikan update sekali lagi pas teks udah full
                Canvas.ForceUpdateCanvases();
                if (bubbleRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
                if (containerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }
        }
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator ShakeEffect(GameObject target, float duration, float magnitude)
    {
        Vector3 originalPos = target.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            target.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.transform.localPosition = originalPos; 
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

    IEnumerator FadeOutUI(GameObject target, float duration)
    {
        if (target == null) yield break;

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();

        float startAlpha = cg.alpha;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        cg.alpha = 0f;
        target.SetActive(false);
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

    private void ShowAllChoicesInstant()
    {
        if (choicePanelWeek3 != null)
        {
            choicePanelWeek3.gameObject.SetActive(true);
            choicePanelWeek3.alpha = 1f;
            SetCanvasGroupInteractable(choicePanelWeek3, true);
        }

        if (optionCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionCanvasGroups)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 1f;
                    SetCanvasGroupInteractable(cg, true);
                }
            }
        }
    }

    private void ResetChoicePanel()
    {
        if (choicePanelWeek3 != null)
        {
            choicePanelWeek3.alpha = 0f; 
            choicePanelWeek3.gameObject.SetActive(false);
        }

        if (optionCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.gameObject.SetActive(false);
                }
            }
        }
    }

    private void SetCanvasGroupInteractable(CanvasGroup cg, bool state)
    {
        if (cg != null)
        {
            cg.interactable = state;
            cg.blocksRaycasts = state;
        }
    }

    private void SetPhoneHeader(string nameText, Sprite profileSprite)
    {
        if (contactNameText != null) contactNameText.text = nameText;
        if (profilePicImage != null && profileSprite != null) profilePicImage.sprite = profileSprite;
    }

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
        }
    }

    private void OnUserTap()
    {
        if (isTyping)
        {
            isTyping = false;
        }
        else
        {
            userTapped = true;
        }
    }

    IEnumerator PlayDialog(string speaker, string text)
    {
        if (dialogBoxObject != null)
        {
            dialogBoxObject.SetActive(true);
            CanvasGroup cg = dialogBoxObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        if (dialogueText != null) dialogueText.text = "";

        EnsureTapButtonActive();
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

        while (!userTapped)
        {
            yield return null;
        }

        userTapped = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}