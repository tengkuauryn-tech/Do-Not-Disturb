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

    [Header("Dialogue System")]
    public GameObject dialogPanel; 
    public TMP_Text dialogueText;  
    public Image dialogBoxBackgroundImage; 
    public Button invisibleTapButton;

    [Header("DialogBox Sprites Per Character")]
    public Sprite dialogBoxKiranaSprite;   
    public Sprite dialogBoxSagaraSprite;   

    [Header("Transition Week 2")]
    public Image blackOverlay;       
    public TMP_Text interludeText;   

    [Header("Choice Panel Week 2")]
    public CanvasGroup choicePanelWeek2;
    public CanvasGroup[] optionCanvasGroups;
    public float delayBeforeFirstButton = 0.4f;
    public float delayBetweenButtons = 0.2f;

    private List<Dictionary<string, string>> csvData;
    private string currentSceneID = "SCENE_02_01";
    private bool userTapped = false;
    private bool isTyping = false;
    private string fullTextHolder = "";
    private Coroutine activeSceneCoroutine;

    public void StartWeek2()
    {
        if (week2Section != null) week2Section.SetActive(true);
        
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
        
        // 1. Setup Awal (Hitam Pekat & Teks Nyala)
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

        // Matikan kelas dan Sagara di awal
        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(false);
        if (sagaraCharacterImage != null) sagaraCharacterImage.gameObject.SetActive(false);

        // 2. Tahan Title Card selama 2 detik
        yield return new WaitForSeconds(2f);

        // 3. FADE OUT TEKS (Cepat)
        float timer = 0f;
        float textFadeTime = 0.4f;
        while (timer < textFadeTime)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(1f, 0f, timer / textFadeTime);
            if (interludeText != null) SetTextAlpha(interludeText, currentAlpha);
            yield return null;
        }
        if (interludeText != null)
        {
            SetTextAlpha(interludeText, 0f);
            interludeText.gameObject.SetActive(false); 
        }

        yield return new WaitForSeconds(0.2f);

        // 4. NYALAKAN KELAS KOSONG (Sagara tetap dimatikan dulu)
        if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
        
        // 5. FADE OUT LAYAR HITAM 
        timer = 0f;
        float bgFadeTime = 0.8f;
        while (timer < bgFadeTime)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(1f, 0f, timer / bgFadeTime);
            if (blackOverlay != null) SetImageAlpha(blackOverlay, currentAlpha);
            yield return null;
        }
        if (blackOverlay != null)
        {
            SetImageAlpha(blackOverlay, 0f);
            blackOverlay.gameObject.SetActive(false); 
        }

        yield return new WaitForSeconds(0.3f);

        // Mulai jalankan CSV (Sagara belum muncul, baru background kelas)
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
                    // Hanya memunculkan background, Sagara tetap diumpetin dulu
                    if (mainBackgroundImage != null) mainBackgroundImage.gameObject.SetActive(true);
                }

                if (nodeType == "DIALOGUE" && !string.IsNullOrEmpty(text))
                {
                    string cleanSpeaker = speaker != null ? speaker.Trim().ToUpper() : "";

                    // DETEKSI OTOMATIS: Kalau ini dialognya Sagara dan gambarnya belum ada, munculin Sagara!
                    if (cleanSpeaker.Contains("SAGARA") && sagaraCharacterImage != null && !sagaraCharacterImage.gameObject.activeSelf)
                    {
                        HideDialogBoxInstant(); // Hilangkan kotak dialog narasi MC sementara
                        
                        sagaraCharacterImage.gameObject.SetActive(true);
                        SetImageAlpha(sagaraCharacterImage, 0f);

                        float timer = 0f;
                        float sagaraFadeTime = 0.5f;
                        while (timer < sagaraFadeTime)
                        {
                            timer += Time.deltaTime;
                            float currentAlpha = Mathf.Lerp(0f, 1f, timer / sagaraFadeTime);
                            SetImageAlpha(sagaraCharacterImage, currentAlpha);
                            yield return null;
                        }
                        SetImageAlpha(sagaraCharacterImage, 1f);
                        yield return new WaitForSeconds(0.3f); // Jeda sebelum teksnya muncul
                    }

                    yield return StartCoroutine(ShowDialogueWithTyping(speaker, text));
                    yield return StartCoroutine(WaitForTap());
                }

                if (trigger == "TRIGGER_CHOICE_02")
                {
                    HideDialogBoxInstant();
                    if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                    yield return StartCoroutine(AnimateButtonsStaggered(optionCanvasGroups));
                    yield break; 
                }

               if (!string.IsNullOrEmpty(nextScene) && nextScene != currentSceneID && nextScene != "END")
                {
                    // Pindah baca ke SceneID yang baru
                    activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV(nextScene));
                    yield break;
                }}
        }
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
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
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
            choicePanelWeek2.alpha = 0f;
            choicePanelWeek2.interactable = false;
            choicePanelWeek2.blocksRaycasts = false;
            choicePanelWeek2.gameObject.SetActive(false);
        }
    }

    IEnumerator AnimateButtonsStaggered(CanvasGroup[] targetButtons)
    {
        yield return new WaitForSeconds(delayBeforeFirstButton);

        if (choicePanelWeek2 != null)
        {
            choicePanelWeek2.gameObject.SetActive(true);
            choicePanelWeek2.alpha = 1f;
            choicePanelWeek2.interactable = true;
            choicePanelWeek2.blocksRaycasts = true;
        }

        if (targetButtons != null)
        {
            foreach (CanvasGroup cg in targetButtons)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    yield return new WaitForSeconds(delayBetweenButtons);
                }
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

    // --- FUNGSI KLIK TOMBOL PILIHAN WEEK 2 ---
    public void OnSelectOption2A()
    {
        DisableChoicePanelWeek2();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_A"));
    }

    public void OnSelectOption2B()
    {
        DisableChoicePanelWeek2();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_B"));
    }

    public void OnSelectOption2C()
    {
        DisableChoicePanelWeek2();
        if (activeSceneCoroutine != null) StopCoroutine(activeSceneCoroutine);
        activeSceneCoroutine = StartCoroutine(PlaySceneFromCSV("CHOICE_02_C"));
    }

    private void DisableChoicePanelWeek2()
    {
        if (choicePanelWeek2 != null)
        {
            choicePanelWeek2.alpha = 0f;
            choicePanelWeek2.interactable = false;
            choicePanelWeek2.blocksRaycasts = false;
            choicePanelWeek2.gameObject.SetActive(false);
        }
    }
}