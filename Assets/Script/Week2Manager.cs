using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Week2Manager : MonoBehaviour
{
    [Header("CSV Settings")]
    public string csvFileName = "WEEK_2_SCRIPT(1)";

    [Header("Main Containers")]
    public GameObject week2Section;
    public Image mainBackgroundImage;
    public Image sagaraCharacterImage;
    public Image kiranaSagaraCG;      
    public Image sagaraIntroCG;      

    [Header("Option C 3 Separate UI Images")]
    public Image optCGiveGlasses;    
    public Image optCWearGlasses;    
    public Image optCFinalSmile;     

    [Header("Global UI / Status Panel")]
    public GameObject statusPanel;    

    [Header("Sprites for Option A")]
    public Sprite kiranaNormalSprite;   
    public Sprite kiranaLookingSprite;  
    public Sprite sagaraSmileSprite;    

    [Header("Sprites for Option B (Path Neutral)")]
    public Sprite sagaraUnderTableSprite; 
    public Sprite sagaraIntroSprite;      

    [Header("Sprites for Option C (Path Close / Akrab)")]
    public Sprite kiranaGiveGlassesSprite; 
    public Sprite sagaraWearGlassesSprite; 
    public Sprite sagaraFinalSmileSprite;  

    [Header("Dialogue System")]
    public GameObject dialogPanel; 
    public TMP_Text dialogueText;  
    public Image dialogBoxBackgroundImage; 
    public Button invisibleTapButton;

    [Header("DialogBox Sprites Per Character")]
    public Sprite dialogBoxKiranaSprite;   
    public Sprite dialogBoxSagaraSprite;   

    [Header("Transition Week 2 & 3")]
    public Image blackOverlay;       
    public TMP_Text interludeText;   
    public Week3Manager week3ScriptTarget; 

    [Header("Choice Panel Week 2")]
    public CanvasGroup choicePanelWeek2;
    public CanvasGroup[] optionCanvasGroups;
    public float delayBeforeFirstButton = 0.2f;
    public float delayBetweenButtons = 0.15f;

    // --- FLAG GLOBAL UNTUK PATH DISTANT (DIBACA DI WEEK 3) ---
    public static bool isPathDistant = false;

    private List<Dictionary<string, string>> csvData;
    private string currentSceneID = "SCENE_02_01";
    private bool userTapped = false;
    private bool isTyping = false;
    private string fullTextHolder = "";
    private Coroutine activeSceneCoroutine;

    public void StartWeek2()
    {
        if (week2Section != null) week2Section.SetActive(true);

        if (statusPanel != null)
        {
            statusPanel.SetActive(true);
        }

        csvData = CSVReader.Read(csvFileName);

        EnsureTapButtonActive();
        if (invisibleTapButton != null)
        {
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        ResetToMainState();

        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(ShowWeek2Transition());
    }

    IEnumerator ShowWeek2Transition()
    {
        HideDialogBoxInstant();
        
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            SetImageAlpha(blackOverlay, 1f); 
        }
        
        if (interludeText != null)
        {
            interludeText.gameObject.SetActive(true);
            interludeText.text = "Week 2 - Do Not Disturb"; 
            SetTextAlpha(interludeText, 1f); 
        }

        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
        if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);
        if (kiranaSagaraCG != null) kiranaSagaraCG.gameObject.SetActive(false);
        if (sagaraIntroCG != null) sagaraIntroCG.gameObject.SetActive(false);
        if (optCGiveGlasses != null) optCGiveGlasses.gameObject.SetActive(false);
        if (optCWearGlasses != null) optCWearGlasses.gameObject.SetActive(false);
        if (optCFinalSmile != null) optCFinalSmile.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f);

        float timer = 0f;
        float textFadeTime = 0.25f;
        while (timer < textFadeTime)
        {
            timer += Time.deltaTime;
            if (interludeText != null) SetTextAlpha(interludeText, Mathf.Lerp(1f, 0f, timer / textFadeTime));
            yield return null;
        }
        if (interludeText != null) interludeText.gameObject.SetActive(false); 

        yield return new WaitForSeconds(0.1f);

        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
        
        timer = 0f;
        float bgFadeTime = 0.4f;
        while (timer < bgFadeTime)
        {
            timer += Time.deltaTime;
            if (blackOverlay != null) SetImageAlpha(blackOverlay, Mathf.Lerp(1f, 0f, timer / bgFadeTime));
            yield return null;
        }
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false); 

        yield return new WaitForSeconds(0.15f);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV(currentSceneID));
    }

    IEnumerator PlaySceneFromCSV(string targetSceneID)
    {
        currentSceneID = targetSceneID;

        for (int i = 0; i < csvData.Count; i++)
        {
            string rowScene = csvData[i]["SceneID"];

            if (rowScene == currentSceneID)
            {
                string nodeType = csvData[i]["NodeType"];
                string speaker = csvData[i]["Speaker"];
                string text = csvData[i]["DialogueText"];
                string trigger = csvData[i]["TriggerAction"];
                string nextScene = csvData[i]["NextSceneID"];

                if (trigger == "SHOW_CLASSROOM")
                {
                    if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                }

                if (nodeType == "DIALOGUE" && !string.IsNullOrEmpty(text))
                {
                    string cleanSpeaker = speaker != null ? speaker.Trim().ToUpper() : "";

                    if (currentSceneID == "CHOICE_02_B" && text.Contains("ketemu"))
                    {
                        string[] parts = text.Split(new string[] { "!" }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            if (kiranaSagaraCG != null && !kiranaSagaraCG.gameObject.activeSelf)
                            {
                                if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
                                kiranaSagaraCG.gameObject.SetActive(true);
                                if (sagaraUnderTableSprite != null) kiranaSagaraCG.sprite = sagaraUnderTableSprite;
                                SetImageAlpha(kiranaSagaraCG, 1f);
                            }
                            yield return StartCoroutine(ShowDialogueWithTyping(speaker, parts[0].Trim() + "!"));
                            yield return StartCoroutine(WaitForTap());

                            if (kiranaSagaraCG != null) kiranaSagaraCG.gameObject.SetActive(false);
                            if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                            if (sagaraIntroCG != null && sagaraIntroSprite != null)
                            {
                                sagaraIntroCG.gameObject.SetActive(true);
                                sagaraIntroCG.sprite = sagaraIntroSprite;
                                SetImageAlpha(sagaraIntroCG, 1f);
                            }
                            yield return StartCoroutine(ShowDialogueWithTyping(speaker, parts[1].Trim()));
                            yield return StartCoroutine(WaitForTap());

                            goto ContinueLoop;
                        }
                    }

                    if (currentSceneID == "CHOICE_02_C" && cleanSpeaker.Contains("SAGARA") && text.Contains("Makasih"))
                    {
                        if (optCGiveGlasses != null) optCGiveGlasses.gameObject.SetActive(false);

                        if (optCWearGlasses != null)
                        {
                            if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
                            optCWearGlasses.gameObject.SetActive(true);
                            if (sagaraWearGlassesSprite != null) optCWearGlasses.sprite = sagaraWearGlassesSprite;
                            SetImageAlpha(optCWearGlasses, 1f);
                        }
                        yield return StartCoroutine(ShowDialogueWithTyping(speaker, "Oh iya benar!"));
                        yield return StartCoroutine(WaitForTap());

                        if (optCWearGlasses != null) optCWearGlasses.gameObject.SetActive(false);
                        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                        if (optCFinalSmile != null)
                        {
                            optCFinalSmile.gameObject.SetActive(true);
                            if (sagaraFinalSmileSprite != null) optCFinalSmile.sprite = sagaraFinalSmileSprite;
                            SetImageAlpha(optCFinalSmile, 1f);
                        }
                        
                        string remainingText = text.Contains("benar!") ? text.Substring(text.IndexOf("benar!") + 6).Trim() : text;
                        yield return StartCoroutine(ShowDialogueWithTyping(speaker, remainingText));
                        yield return StartCoroutine(WaitForTap());

                        goto ContinueLoop;
                    }

                    if (cleanSpeaker.Contains("SAGARA"))
                    {
                        if (text.Contains("Permisi"))
                        {
                            if (kiranaSagaraCG != null && !kiranaSagaraCG.gameObject.activeSelf)
                            {
                                kiranaSagaraCG.gameObject.SetActive(true);
                                if (kiranaNormalSprite != null) kiranaSagaraCG.sprite = kiranaNormalSprite;
                                SetImageAlpha(kiranaSagaraCG, 1f);
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
                            }
                        }
                        else if (text.Contains("kacamata") && currentSceneID == "SCENE_02_01")
                        {
                            if (sagaraCharacterImage != null && !sagaraCharacterImage.gameObject.activeSelf)
                            {
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                                sagaraCharacterImage.gameObject.SetActive(true);
                                SetImageAlpha(sagaraCharacterImage, 1f);

                                if (kiranaSagaraCG != null)
                                {
                                    SetImageAlpha(kiranaSagaraCG, 0f);
                                    kiranaSagaraCG.gameObject.SetActive(false);
                                }
                            }
                        }
                    }

                    if (currentSceneID == "CHOICE_02_A")
                    {
                        if (text.Contains("Apasih") || text.Contains("ganggu"))
                        {
                            if (kiranaSagaraCG != null && !kiranaSagaraCG.gameObject.activeSelf)
                            {
                                if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);

                                kiranaSagaraCG.gameObject.SetActive(true);
                                if (kiranaNormalSprite != null) kiranaSagaraCG.sprite = kiranaNormalSprite;
                                SetImageAlpha(kiranaSagaraCG, 1f);
                            }
                        }
                        else if (text.Contains("Ternyata ada di tas"))
                        {
                            if (kiranaSagaraCG != null && kiranaLookingSprite != null) kiranaSagaraCG.sprite = kiranaLookingSprite;
                        }
                        else if (text.Contains("Siapa sih dia"))
                        {
                            if (kiranaSagaraCG != null) kiranaSagaraCG.gameObject.SetActive(false);
                            if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                            if (sagaraCharacterImage != null && sagaraSmileSprite != null)
                            {
                                sagaraCharacterImage.gameObject.SetActive(true);
                                sagaraCharacterImage.sprite = sagaraSmileSprite;
                                SetImageAlpha(sagaraCharacterImage, 1f);
                            }
                        }
                    }

                    if (currentSceneID == "CHOICE_02_B")
                    {
                        if (text.Contains("nyari kacamata") || text.Contains("Iya"))
                        {
                            if (kiranaSagaraCG != null && !kiranaSagaraCG.gameObject.activeSelf)
                            {
                                if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);

                                kiranaSagaraCG.gameObject.SetActive(true);
                                if (sagaraUnderTableSprite != null) kiranaSagaraCG.sprite = sagaraUnderTableSprite;
                                SetImageAlpha(kiranaSagaraCG, 1f);
                            }
                        }
                    }

                    if (currentSceneID == "CHOICE_02_C")
                    {
                        if (text.Contains("kamu bukan"))
                        {
                            if (optCGiveGlasses != null && !optCGiveGlasses.gameObject.activeSelf)
                            {
                                if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);
                                if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);

                                optCGiveGlasses.gameObject.SetActive(true);
                                if (kiranaGiveGlassesSprite != null) optCGiveGlasses.sprite = kiranaGiveGlassesSprite;
                                SetImageAlpha(optCGiveGlasses, 1f);
                            }
                        }
                    }

                    yield return StartCoroutine(ShowDialogueWithTyping(speaker, text));
                    yield return StartCoroutine(WaitForTap());
                }

                ContinueLoop:

                if (!string.IsNullOrEmpty(trigger) && trigger.Contains("TRIGGER_CHOICE"))
                {
                    HideDialogBoxInstant();
                    if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                    yield return StartCoroutine(AnimateButtonsStaggered(optionCanvasGroups));
                    yield break; 
                }

                if (nextScene == "END" || string.IsNullOrEmpty(nextScene))
                {
                    HideDialogBoxInstant();
                    if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                    
                    yield return StartCoroutine(TransitionToWeek3Routine());
                    yield break;
                }

                if (!string.IsNullOrEmpty(nextScene) && nextScene != currentSceneID)
                {
                    activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV(nextScene));
                    yield break;
                }
            }
        }
    }

    IEnumerator TransitionToWeek3Routine()
    {
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                SetImageAlpha(blackOverlay, Mathf.Lerp(0f, 1f, t));
                yield return null;
            }
            SetImageAlpha(blackOverlay, 1f);
        }

        if (interludeText != null)
        {
            interludeText.gameObject.SetActive(true);
            interludeText.text = "Week 3: Mid-Semester Pressure";
            SetTextAlpha(interludeText, 1f);
        }

        yield return new WaitForSeconds(2.5f);

        if (interludeText != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                SetTextAlpha(interludeText, Mathf.Lerp(1f, 0f, t));
                yield return null;
            }
            interludeText.gameObject.SetActive(false);
        }

        if (week2Section != null) week2Section.SetActive(false);

        if (week3ScriptTarget != null)
        {
            week3ScriptTarget.gameObject.SetActive(true);
            week3ScriptTarget.StartWeek3();
        }

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(false);
        }

        enabled = false;
    }

    IEnumerator ShowDialogueWithTyping(string speaker, string text)
    {
        EnsureTapButtonActive();

        if (dialogBoxBackgroundImage != null)
        {
            string cleanSpeaker = speaker != null ? speaker.Trim().ToUpper() : "";

            if ((cleanSpeaker.Contains("SAGARA") || cleanSpeaker.Contains("TARA")) && dialogBoxSagaraSprite != null)
            {
                dialogBoxBackgroundImage.sprite = dialogBoxSagaraSprite;
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
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }

        fullTextHolder = text;
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char letter in text)
        {
            if (!isTyping) break; 
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(0.04f);
        }

        if (dialogueText != null) dialogueText.text = fullTextHolder;
        isTyping = false;
        
        yield return new WaitForSeconds(0.15f);
        userTapped = false; 
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

    private void HideDialogBoxInstant()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
    }

    private void EnsureTapButtonActive()
    {
        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTapButton.transform.SetAsLastSibling();
        }
    }

    private void ResetToMainState()
    {
        HideDialogBoxInstant();
        if (choicePanelWeek2 != null)
        {
            choicePanelValueLocal(); 
        }
    }

    private void choicePanelValueLocal()
    {
        if (choicePanelWeek2 != null)
        {
            choicePanelWeek2.alpha = 0f; choicePanelWeek2.interactable = false;
            choicePanelWeek2.blocksRaycasts = false; choicePanelWeek2.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateButtonsStaggered(CanvasGroup[] targetButtons)
    {
        yield return new WaitForSeconds(delayBeforeFirstButton);
        if (choicePanelWeek2 != null)
        {
            choicePanelWeek2.gameObject.SetActive(true); choicePanelWeek2.alpha = 1f;
            choicePanelWeek2.interactable = true; choicePanelWeek2.blocksRaycasts = true;
        }
        if (targetButtons != null)
        {
            foreach (CanvasGroup cg in targetButtons)
            {
                if (cg != null) { cg.gameObject.SetActive(true); cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; yield return new WaitForSeconds(delayBetweenButtons); }
            }
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
    }

    private void SetTextAlpha(TMP_Text txt, float alpha)
    {
        if (txt != null) txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, alpha);
    }

    // --- PILIHAN A DI WEEK 2 (MENGAKTIFKAN FLAG DISTANT) ---
    public void OnSelectOption2A() 
    { 
        isPathDistant = true; // Sagara dikunci di Week 3!
        DisableChoicePanelWeek2(); 
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine); 
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_A")); 
    }

    public void OnSelectOption2B() { DisableChoicePanelWeek2(); if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine); activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_B")); }
    public void OnSelectOption2C() { DisableChoicePanelWeek2(); if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine); activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_C")); }

    private void DisableChoicePanelWeek2()
    {
        if (choicePanelWeek2 != null) { choicePanelWeek2.alpha = 0f; choicePanelWeek2.interactable = false; choicePanelWeek2.blocksRaycasts = false; choicePanelWeek2.gameObject.SetActive(false); }
    }
}