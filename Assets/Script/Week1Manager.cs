using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Week1Manager : MonoBehaviour
{
    #region CSV & MAIN CONTAINERS
    [Header("CSV Settings")]
    public string csvFileName = "WEEK_1_SCRIPT";

    [Header("Main Containers")]
    public GameObject week1Section;
    public GameObject phoneMainListChat;   
    public GameObject phoneViewInsideChat; 
    public GameObject phoneViewScreenOff;  
    public GameObject statusPanel;         

    [Header("Animated Hand & Phone Transform")]
    public Transform insidePhoneTransform; 
    public float phoneZoomDuration = 0.35f;
    #endregion

    #region SCENE 1.1 & 1.2 CHOICE PANELS
    [Header("Choice Panel 1 (Scene 01.01)")]
    public CanvasGroup choicePanelCanvasGroup; 
    public CanvasGroup[] optionCanvasGroups;   
    public float delayBeforeFirstButton = 0.4f;
    public float delayBetweenButtons = 0.2f;
    public float buttonFadeDuration = 0.3f;

    [Header("Choice Panel 2 - Tara (Scene 01.02)")]
    public CanvasGroup choicePanelTaraCanvasGroup;
    public CanvasGroup[] optionTaraCanvasGroups;
    #endregion

    #region SCENE 1.3 ASSETS & CHOICE PANEL
    [Header("Scene 1.3 - Choice Panel")]
    public CanvasGroup choicePanel1_3CanvasGroup;
    public CanvasGroup[] option1_3CanvasGroups;

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
    #endregion

    #region CHAT CONTAINERS & BUBBLES
    [Header("Inside Chat Containers")]
    public GameObject dosenChatContent;    
    public GameObject grupChatContent;     
    public GameObject temanChatContent;    

    [Header("Dosen Chat Bubbles (Option A)")]
    public GameObject dosenBubbleLeft;     
    public GameObject dosenBubbleRight;    

    [Header("Grup Chat Bubbles (Option B)")]
    public GameObject grupBubbleLeft;      
    public GameObject grupBubbleRight;     

    [Header("Tara Chat Bubbles (Scene 01.02)")]
    public GameObject taraBubble1_Left;    
    public GameObject taraBubble2_Left;    
    public GameObject mcReplyBubble_Right; 
    public GameObject taraReplyBubble_Left; 
    #endregion

    #region DESK & INTERLUDE ASSETS
    [Header("Option C Desk Assets")]
    public Image blackOverlay;             
    public TMP_Text interludeText;         
    public GameObject tablePhoneView;      
    public GameObject tablePhoneNotifOn;    
    #endregion

    #region GLOBAL UI & DIALOGUE SYSTEM
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

    [Header("Header Elements")]
    public TMP_Text headerNameText;       
    public Image headerProfileImage;      
    public Sprite dosenProfileSprite;     
    public Sprite grupProfileSprite;      
    public Sprite taraProfileSprite;      

    [Header("DND Mechanic Controls")]
    public Button dndToggleButton;
    public Image dndToggleImage;           
    public Sprite dndOnSprite;             
    public Sprite dndOffSprite;            
    public GameObject dndStatusIcon;       
    
    [HideInInspector]
    public bool isDndActive = false;       

    [Header("Audio SFX")]
    public AudioSource sfxSource;
    public AudioClip messageSFX;
    public AudioClip beepSFX;

    [Header("Typing & Transition Settings")]
    public float typingSpeed = 0.04f;
    public float initialDelayBeforeDialogue = 1.0f;
    public float dialogueFadeInDuration = 0.3f;

    private bool userTapped = false;
    private bool isTyping = false;
    private string fullTextHolder = "";

    private List<Dictionary<string, string>> csvData;
    private string currentSceneID = "SCENE_01_01";
    private Coroutine activeSceneCoroutine;
    #endregion

    #region INITIALIZATION
    public void StartWeek1()
    {
        if (week1Section != null) week1Section.SetActive(true);
        if (statusPanel != null) statusPanel.SetActive(true);

        csvData = CSVReader.Read(csvFileName);

        EnsureTapButtonActive();
        if (invisibleTapButton != null)
        {
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        ResetToMainList();

        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV(currentSceneID));
    }
    #endregion

    #region MAIN SCENE RUNNER (CSV)
    IEnumerator PlaySceneFromCSV(string targetSceneID)
    {
        currentSceneID = targetSceneID;

        if (currentSceneID == "SCENE_01_02")
        {
            HideDialogBoxInstant();
            yield return StartCoroutine(RunScene0102TaraSequence());
            yield break;
        }

        if (currentSceneID == "SCENE_01_03")
        {
            HideDialogBoxInstant();
            SetScene1_3Setup();
        }

        for (int i = 0; i < csvData.Count; i++)
        {
            string rowScene = csvData[i]["SceneID"];

            if (rowScene == currentSceneID)
            {
                string nodeType = csvData[i]["NodeType"];
                string speaker = csvData[i]["Speaker"];
                string text = csvData[i]["DialogueText"];
                string trigger = csvData[i]["TriggerAction"];
                string sfx = csvData[i]["SFX"];
                string nextScene = csvData[i]["NextSceneID"];

                if (sfx == "beep") PlaySFX(beepSFX);
                else if (sfx == "message") PlaySFX(messageSFX);

                if (trigger == "SHOW_CHAT_DOSEN")
                {
                    HideDialogBoxInstant();
                    yield return StartCoroutine(RunOptionASequence());
                    yield break;
                }
                else if (trigger == "SHOW_CHAT_GRUP")
                {
                    HideDialogBoxInstant();
                    yield return StartCoroutine(RunOptionBSequence());
                    yield break;
                }
                else if (trigger == "SHOW_CHAT_TARA" || trigger == "SHOW_WA_INTERFACE")
                {
                    HideDialogBoxInstant();
                    yield return StartCoroutine(RunScene0102TaraSequence());
                    yield break;
                }
                else if (trigger == "FLIP_HP")
                {
                    HideDialogBoxInstant();
                    yield return StartCoroutine(RunOptionCSequence());
                    yield break;
                }
                else if (trigger == "DND_ACTIVE")
                {
                    HideDialogBoxInstant();
                    ToggleDNDState();
                    yield break;
                }
                else if (trigger == "TRIGGER_CHOICE_03" || trigger == "TRIGGER_CHOICE_1_3")
                {
                    HideDialogBoxInstant();
                    if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                    ShowChoicePanel1_3();
                    yield break;
                }

                if (nodeType == "DIALOGUE" && !string.IsNullOrEmpty(text))
                {
                    if (targetSceneID == "SCENE_01_01")
                    {
                        yield return new WaitForSeconds(initialDelayBeforeDialogue);
                    }

                    yield return StartCoroutine(ShowDialogueWithTyping(speaker, text));
                    yield return StartCoroutine(WaitForTap());

                    if (trigger.StartsWith("TRIGGER_CHOICE"))
                    {
                        HideDialogBoxInstant();
                    }
                }

                if (trigger.StartsWith("TRIGGER_CHOICE") && trigger != "TRIGGER_CHOICE_03" && trigger != "TRIGGER_CHOICE_1_3")
                {
                    HideDialogBoxInstant();
                    if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                    StartCoroutine(AnimateButtonsStaggered(optionCanvasGroups));
                    yield break;
                }

                if (!string.IsNullOrEmpty(nextScene) && nextScene != currentSceneID)
                {
                    if (nextScene == "END")
                    {
                        yield return StartCoroutine(TransitionToSceneHendra());
                        yield break;
                    }

                    activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV(nextScene));
                    yield break;
                }
            }
        }
    }
    #endregion

    #region SCENE 01.03 LOGIC (RUANG KELAS & PAK HENDRA)
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

    private void ShowChoicePanel1_3()
    {
        if (choicePanel1_3CanvasGroup != null)
        {
            choicePanel1_3CanvasGroup.gameObject.SetActive(true);
            choicePanel1_3CanvasGroup.alpha = 1f;
            SetCanvasGroupInteractable(choicePanel1_3CanvasGroup, true);
        }

        StartCoroutine(AnimateButtonsStaggered(option1_3CanvasGroups));
    }

    private void HideChoicePanel1_3()
    {
        if (choicePanel1_3CanvasGroup != null)
        {
            choicePanel1_3CanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanel1_3CanvasGroup, false);
            choicePanel1_3CanvasGroup.gameObject.SetActive(false);
        }
    }

    public void OnSelectOption3A()
    {
        HideDialogBoxInstant(); // Sembunyikan dialog box agar tidak nyangkut frame sebelumnya
        HideChoicePanel1_3();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption3ASequence());
    }

    IEnumerator RunOption3ASequence()
    {
        if (actionOverlayImage != null && spriteMcCatat != null)
        {
            actionOverlayImage.sprite = spriteMcCatat;
            actionOverlayImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null && hendraPraiseSprite != null)
        {
            hendraCharacterImage.sprite = hendraPraiseSprite;
            hendraCharacterImage.SetNativeSize();
        }

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Bagus Kirana, lanjutkan"));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToNextSection());
    }

    public void OnSelectOption3B()
    {
        HideDialogBoxInstant();
        HideChoicePanel1_3();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption3BSequence());
    }

    IEnumerator RunOption3BSequence()
    {
        if (actionOverlayImage != null && spriteMcLihatHp != null)
        {
            actionOverlayImage.sprite = spriteMcLihatHp;
            actionOverlayImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null && hendraSilentStareSprite != null)
        {
            hendraCharacterImage.sprite = hendraSilentStareSprite;
            hendraCharacterImage.SetNativeSize();
        }

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "(Pak Hendra berhenti sebentar, menatap MC tanpa berkata apa-apa)"));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToNextSection());
    }

    public void OnSelectOption3C()
    {
        HideDialogBoxInstant();
        HideChoicePanel1_3();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption3CSequence());
    }

    IEnumerator RunOption3CSequence()
    {
        if (actionOverlayImage != null && spriteMcAngkatTangan != null)
        {
            actionOverlayImage.sprite = spriteMcAngkatTangan;
            actionOverlayImage.gameObject.SetActive(true);
        }

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Maaf pak, izin bertanya. Kalau kita mendapatkan informasi seperti itu, mending diam saja atau bagaimana ya pak?"));
        yield return StartCoroutine(WaitForTap());

        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null && hendraTegasSprite != null)
        {
            hendraCharacterImage.sprite = hendraTegasSprite;
            hendraCharacterImage.SetNativeSize();
        }

        yield return StartCoroutine(ShowDialogueWithTyping("Pak Hendra", "Tanya sumber pertama. Jangan ikut menyebarkan sesuatu yang bahkan belum kamu pahami."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToNextSection());
    }

    private IEnumerator TransitionToNextSection()
    {
        yield return StartCoroutine(FadeOutDialogBoxSmooth(0.3f));
        if (hendraCharacterImage != null) hendraCharacterImage.gameObject.SetActive(false);
        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
    }
    #endregion

    #region DIALOGUE SYSTEM & TYPING
    IEnumerator ShowDialogueWithTyping(string speaker, string text)
    {
        EnsureTapButtonActive();

        if (dialogBoxBackgroundImage != null)
        {
            string cleanSpeaker = speaker != null ? speaker.Trim().ToUpper() : "";

            // HANYA pakai frame Pak Hendra kalau speakernya benar-benar Pak Hendra
            if ((cleanSpeaker.Contains("HENDRA") || cleanSpeaker.Contains("PAK")) && dialogBoxHendraSprite != null)
            {
                dialogBoxBackgroundImage.sprite = dialogBoxHendraSprite;
            }
            else if ((cleanSpeaker.Contains("SAGARA") || cleanSpeaker.Contains("TARA")) && dialogBoxTaraSprite != null)
            {
                dialogBoxBackgroundImage.sprite = dialogBoxTaraSprite;
            }
            else if (dialogBoxKiranaSprite != null)
            {
                dialogBoxBackgroundImage.sprite = dialogBoxKiranaSprite;
            }
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
                else if (characterNameText != null)
                {
                    characterNameText.text = speaker;
                }
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
    #endregion

    #region SCENE 01.01 & 01.02 SEQUENCES
    public void OnSelectOptionA() 
    {
        HideDialogBoxInstant();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOptionASequence());
    }

    IEnumerator RunOptionASequence()
    {
        ClearAllChatBubbles();
        yield return StartCoroutine(PrepareInsideChatView("Pak Hendra", dosenProfileSprite, dosenChatContent));
        EnsureTapButtonActive();

        if (dosenBubbleLeft != null) 
        {
            yield return StartCoroutine(TypeChatBubble(dosenBubbleLeft, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        if (dosenBubbleRight != null) 
        {
            yield return StartCoroutine(TypeChatBubble(dosenBubbleRight, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        for (int i = 0; i < csvData.Count; i++)
        {
            if (csvData[i]["SceneID"] == "CHOICE_01_A" && csvData[i]["NodeType"] == "DIALOGUE")
            {
                yield return StartCoroutine(ShowDialogueWithTyping(csvData[i]["Speaker"], csvData[i]["DialogueText"]));
                yield return StartCoroutine(WaitForTap());
                break;
            }
        }

        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOptionB() 
    {
        HideDialogBoxInstant();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOptionBSequence());
    }

    IEnumerator RunOptionBSequence()
    {
        ClearAllChatBubbles();
        yield return StartCoroutine(PrepareInsideChatView("TMD 5B ASIK", grupProfileSprite, grupChatContent));
        EnsureTapButtonActive();

        if (grupBubbleLeft != null) 
        {
            yield return StartCoroutine(TypeChatBubble(grupBubbleLeft, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        if (grupBubbleRight != null)
        {
            yield return StartCoroutine(TypeChatBubble(grupBubbleRight, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        for (int i = 0; i < csvData.Count; i++)
        {
            if (csvData[i]["SceneID"] == "CHOICE_01_B" && csvData[i]["NodeType"] == "DIALOGUE")
            {
                yield return StartCoroutine(ShowDialogueWithTyping(csvData[i]["Speaker"], csvData[i]["DialogueText"]));
                yield return StartCoroutine(WaitForTap());
                break;
            }
        }

        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOptionC() 
    {
        HideDialogBoxInstant();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOptionCSequence());
    }

    IEnumerator RunOptionCSequence()
    {
        ClearAllChatBubbles();

        if (choicePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(choicePanelCanvasGroup, 1f, 0f, 0.25f));
            SetCanvasGroupInteractable(choicePanelCanvasGroup, false);
        }

        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(false);
        
        if (tablePhoneView != null) tablePhoneView.SetActive(true);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        if (blackOverlay != null) blackOverlay.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, 0.5f));

        if (interludeText != null)
        {
            interludeText.text = "Beberapa saat kemudian...";
            interludeText.gameObject.SetActive(true);
        }
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
        StartCoroutine(TransitionToSceneHendra());
    }

    IEnumerator RunScene0102TaraSequence()
    {
        ClearAllChatBubbles();
        Sprite profileToUse = taraProfileSprite != null ? taraProfileSprite : dosenProfileSprite;
        yield return StartCoroutine(PrepareInsideChatView("Sagara", profileToUse, temanChatContent));

        EnsureTapButtonActive();

        if (taraBubble1_Left != null)
        {
            SetBubbleText(taraBubble1_Left, "Eh, lu masuk kelompok gue ya.");
            yield return StartCoroutine(TypeChatBubble(taraBubble1_Left, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        if (taraBubble2_Left != null)
        {
            SetBubbleText(taraBubble2_Left, "Presentasi minggu depan. Nanti gue kirim pembagian tugas.");
            yield return StartCoroutine(TypeChatBubble(taraBubble2_Left, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        HideDialogBoxInstant();
        if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);

        if (choicePanelCanvasGroup != null)
        {
            choicePanelCanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanelCanvasGroup, false);
            choicePanelCanvasGroup.gameObject.SetActive(false);
        }

        if (choicePanelTaraCanvasGroup != null)
        {
            choicePanelTaraCanvasGroup.gameObject.SetActive(true);
            choicePanelTaraCanvasGroup.alpha = 1f;
            SetCanvasGroupInteractable(choicePanelTaraCanvasGroup, true);
        }

        if (optionTaraCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionTaraCanvasGroups)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 0f;
                }
            }
        }

        yield return StartCoroutine(AnimateButtonsStaggered(optionTaraCanvasGroups));
    }

    private void DisableChoicePanelTara()
    {
        if (choicePanelTaraCanvasGroup != null)
        {
            choicePanelTaraCanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanelTaraCanvasGroup, false);
            choicePanelTaraCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void OnSelectOption2A() 
    {
        HideDialogBoxInstant();
        DisableChoicePanelTara();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption2ASequence());
    }

    IEnumerator RunOption2ASequence()
    {
        EnsureTapButtonActive();

        if (mcReplyBubble_Right != null)
        {
            SetBubbleText(mcReplyBubble_Right, "Ya, kirim aja.");
            yield return StartCoroutine(TypeChatBubble(mcReplyBubble_Right, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        if (taraReplyBubble_Left != null)
        {
            SetBubbleText(taraReplyBubble_Left, "Sip.");
            yield return StartCoroutine(TypeChatBubble(taraReplyBubble_Left, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOption2B() 
    {
        HideDialogBoxInstant();
        DisableChoicePanelTara();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption2BSequence());
    }

    IEnumerator RunOption2BSequence()
    {
        EnsureTapButtonActive();

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "Jawab ntar aja dah."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(ShowDialogueWithTyping("Kirana", "'Nanti'. Satu kata yang sering terdengar seperti solusi, padahal kadang cuma cara menunda masalah."));
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToSceneHendra());
    }

    public void OnSelectOption2C() 
    {
        HideDialogBoxInstant();
        DisableChoicePanelTara();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(RunOption2CSequence());
    }

    IEnumerator RunOption2CSequence()
    {
        EnsureTapButtonActive();

        if (mcReplyBubble_Right != null)
        {
            SetBubbleText(mcReplyBubble_Right, "Presentasi kapan? Bagian gue apa?");
            yield return StartCoroutine(TypeChatBubble(mcReplyBubble_Right, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        if (taraReplyBubble_Left != null)
        {
            SetBubbleText(taraReplyBubble_Left, "Kerjain bagian pendahuluan aja.");
            yield return StartCoroutine(TypeChatBubble(taraReplyBubble_Left, 0.045f));
            PlaySFX(messageSFX);
        }
        yield return StartCoroutine(WaitForTap());

        yield return StartCoroutine(TransitionToSceneHendra());
    }
    #endregion

    #region HELPER & TRANSITIONS
    private void ClearAllChatBubbles()
    {
        if (dosenBubbleLeft != null) dosenBubbleLeft.SetActive(false);
        if (dosenBubbleRight != null) dosenBubbleRight.SetActive(false);

        if (grupBubbleLeft != null) grupBubbleLeft.SetActive(false);
        if (grupBubbleRight != null) grupBubbleRight.SetActive(false);

        if (taraBubble1_Left != null) taraBubble1_Left.SetActive(false);
        if (taraBubble2_Left != null) taraBubble2_Left.SetActive(false);
        if (mcReplyBubble_Right != null) mcReplyBubble_Right.SetActive(false);
        if (taraReplyBubble_Left != null) taraReplyBubble_Left.SetActive(false);
    }

    private void HideDialogBoxInstant()
    {
        if (dialogPanel != null)
        {
            CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            dialogPanel.SetActive(false);
        }
    }

    private IEnumerator FadeOutDialogBoxSmooth(float duration)
    {
        if (dialogPanel != null && dialogPanel.activeSelf)
        {
            CanvasGroup cg = dialogPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, duration));
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            dialogPanel.SetActive(false);
        }
    }

    private IEnumerator TransitionToSceneHendra()
    {
        yield return StartCoroutine(FadeOutDialogBoxSmooth(0.3f));

        if (interludeText != null) interludeText.gameObject.SetActive(false);

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, 0.4f));
        }

        if (tablePhoneView != null) tablePhoneView.SetActive(false);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(false);
        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        ClearAllChatBubbles();

        if (choicePanelTaraCanvasGroup != null)
        {
            choicePanelTaraCanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanelTaraCanvasGroup, false);
            choicePanelTaraCanvasGroup.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.2f);

        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("SCENE_01_03"));

        yield return new WaitForSeconds(0.2f);

        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.4f));
            blackOverlay.gameObject.SetActive(false);
        }
    }

    public void OnUserTap()
    {
        if (isTyping) isTyping = false;
        else userTapped = true;
    }

    IEnumerator WaitForTap()
    {
        userTapped = false;
        while (!userTapped) yield return null;
        userTapped = false;
    }

    IEnumerator TypeChatBubble(GameObject bubbleObject, float speed = 0.045f)
    {
        if (bubbleObject == null) yield break;

        bubbleObject.SetActive(true);
        TMP_Text bubbleText = bubbleObject.GetComponentInChildren<TMP_Text>();

        if (bubbleText != null)
        {
            string fullText = bubbleText.text;
            if (string.IsNullOrEmpty(fullText)) fullText = fullTextHolder;

            fullTextHolder = fullText;
            isTyping = true;
            bubbleText.text = "";

            foreach (char letter in fullText)
            {
                if (!isTyping) break;
                bubbleText.text += letter;
                yield return new WaitForSeconds(speed);
            }

            bubbleText.text = fullTextHolder;
            isTyping = false;
        }
    }

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTapButton.transform.SetAsLastSibling();
        }
    }

    public void ResetToMainList()
    {
        if (week1Section != null) week1Section.SetActive(true);
        if (phoneMainListChat != null) phoneMainListChat.SetActive(true);
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(false);
        if (phoneViewScreenOff != null) phoneViewScreenOff.SetActive(false);
        HideDialogBoxInstant();
        if (tablePhoneView != null) tablePhoneView.SetActive(false);
        if (tablePhoneNotifOn != null) tablePhoneNotifOn.SetActive(false);

        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
        if (hendraCharacterImage != null) hendraCharacterImage.gameObject.SetActive(false);
        if (actionOverlayImage != null) actionOverlayImage.gameObject.SetActive(false);

        isDndActive = false;
        if (dndToggleImage != null && dndOffSprite != null) dndToggleImage.sprite = dndOffSprite;

        ClearAllChatBubbles();

        if (choicePanelCanvasGroup != null)
        {
            choicePanelCanvasGroup.gameObject.SetActive(true);
            choicePanelCanvasGroup.alpha = 1f;
            SetCanvasGroupInteractable(choicePanelCanvasGroup, true);
        }

        if (choicePanelTaraCanvasGroup != null)
        {
            choicePanelTaraCanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanelTaraCanvasGroup, false);
            choicePanelTaraCanvasGroup.gameObject.SetActive(false);
        }

        if (choicePanel1_3CanvasGroup != null)
        {
            choicePanel1_3CanvasGroup.alpha = 0f;
            SetCanvasGroupInteractable(choicePanel1_3CanvasGroup, false);
            choicePanel1_3CanvasGroup.gameObject.SetActive(false);
        }

        if (optionCanvasGroups != null)
        {
            foreach (CanvasGroup cg in optionCanvasGroups)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    SetCanvasGroupInteractable(cg, false);
                    cg.alpha = 0f;
                }
            }
        }
    }

    IEnumerator AnimateButtonsStaggered(CanvasGroup[] targetButtons)
    {
        yield return new WaitForSeconds(delayBeforeFirstButton);

        if (targetButtons != null)
        {
            foreach (CanvasGroup cg in targetButtons)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 1f, buttonFadeDuration));
                    SetCanvasGroupInteractable(cg, true);
                    yield return new WaitForSeconds(delayBetweenButtons);
                }
            }
        }
    }

    IEnumerator PrepareInsideChatView(string headerTitle, Sprite profileSprite, GameObject activeContent)
    {
        if (choicePanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(choicePanelCanvasGroup, choicePanelCanvasGroup.alpha, 0f, 0.25f));
            SetCanvasGroupInteractable(choicePanelCanvasGroup, false);
        }

        if (phoneMainListChat != null) phoneMainListChat.SetActive(false);
        if (phoneViewInsideChat != null) phoneViewInsideChat.SetActive(true);

        if (dosenChatContent != null) dosenChatContent.SetActive(dosenChatContent == activeContent);
        if (grupChatContent != null) grupChatContent.SetActive(grupChatContent == activeContent);
        if (temanChatContent != null) temanChatContent.SetActive(temanChatContent == activeContent);

        if (headerNameText != null) headerNameText.text = headerTitle;
        if (headerProfileImage != null && profileSprite != null) headerProfileImage.sprite = profileSprite;

        if (insidePhoneTransform != null) StartCoroutine(AnimatePhoneScale(insidePhoneTransform));
    }

    IEnumerator AnimatePhoneScale(Transform targetTransform)
    {
        Vector3 initialScale = new Vector3(0.95f, 0.95f, 1f);
        Vector3 targetScale = Vector3.one;
        targetTransform.localScale = initialScale;

        float timer = 0f;
        while (timer < phoneZoomDuration)
        {
            timer += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(initialScale, targetScale, timer / phoneZoomDuration);
            yield return null;
        }
        targetTransform.localScale = targetScale;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        if (cg == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
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

    private void SetCanvasGroupInteractable(CanvasGroup cg, bool state)
    {
        if (cg == null) return;
        cg.interactable = state;
        cg.blocksRaycasts = state;
    }

    private void SetBubbleText(GameObject bubbleObj, string newText)
    {
        if (bubbleObj == null) return;
        TMP_Text t = bubbleObj.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = newText;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }
    #endregion
}