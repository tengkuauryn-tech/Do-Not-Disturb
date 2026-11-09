using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Week5Manager : MonoBehaviour
{
    [Header("Main Containers")]
    public GameObject week5Section; 

    [Header("Visual Layers (Awal Scene Kafe)")]
    public GameObject bgCafe;                  
    public GameObject kiranaSagaraVisual;      

    [Header("Visual Layers (Pilihan Route)")]
    public GameObject optionASagara;           
    public GameObject optionB1;                
    public GameObject optionB2;                
    public GameObject noRomanceVisual;         
    public GameObject laptopStudyVisual; // <--- SLOT BARU UNTUK BACKGROUND LAPTOP

    [Header("Choice Panel, Blur & Buttons")]
    public CanvasGroup choicePanelWeek5;       
    public GameObject overlayBlur;             
    public Button buttonAText;                 
    public Button buttonBText;                 

    [Header("Dialogue System (GLOBAL UI)")]
    public GameObject dialogBoxObject;     
    public TMP_Text dialogueText;          
    public Button invisibleTapButton;      
    public Sprite dialogBoxKiranaSprite;   
    public Sprite dialogBoxSagaraSprite;       

    [Header("Interlude / Overlay Assets")]
    public Image blackOverlay;               
    public TMP_Text weekTitleText;           

    [Header("CSV Database")]
    public TextAsset week5CsvFile;             

    private Image dialogBoxImageComponent;
    private bool userTapped = false;
    private bool isTyping = false;

    private struct DialogueRow
    {
        public string sceneID;
        public string nodeType;
        public string speaker;
        public string dialogueText;
        public string triggerAction;
        public string nextSceneID;
    }

    private List<DialogueRow> csvRows = new List<DialogueRow>();
    private int currentCsvIndex = 0;

    void Awake()
    {
        if (dialogBoxObject != null)
        {
            dialogBoxImageComponent = dialogBoxObject.GetComponent<Image>();
        }
        ResetChoicePanel();
        ParseCSV();
    }

    void ParseCSV()
    {
        if (week5CsvFile == null) return;

        csvRows.Clear();
        string[] lines = week5CsvFile.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            List<string> fields = ParseCsvLine(line);
            
            if (fields.Count >= 6)
            {
                DialogueRow row;
                row.sceneID = fields[0].Trim().Trim('"');
                row.nodeType = fields[1].Trim().Trim('"');
                row.speaker = fields[2].Trim().Trim('"');
                row.dialogueText = fields[3].Trim().Trim('"');
                row.triggerAction = fields[4].Trim().Trim('"');
                row.nextSceneID = fields[5].Trim().Trim('"');

                csvRows.Add(row);
            }
        }
    }

    List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        result.Add(currentField);
        return result;
    }

    public void StartWeek5()
    {
        if (week5Section != null) week5Section.SetActive(true);

        Week4Manager w4 = FindFirstObjectByType<Week4Manager>(FindObjectsInactive.Include);
        if (w4 != null) 
        { 
            if (w4.week4Section != null) w4.week4Section.SetActive(false);
            w4.StopAllCoroutines(); 
            w4.enabled = false; 
        }

        if (dialogueText != null) dialogueText.text = "";

        if (invisibleTapButton != null)
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTapButton.onClick.RemoveAllListeners();
            invisibleTapButton.onClick.AddListener(OnUserTap);
        }

        bool isCoffeeDateAllowed = !Week2Manager.isPathDistant; 

        StopAllCoroutines();

        if (isCoffeeDateAllowed)
        {
            if (bgCafe) bgCafe.SetActive(true);
            if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(true);
            
            if (optionASagara) optionASagara.SetActive(false);
            if (optionB1) optionB1.SetActive(false);
            if (optionB2) optionB2.SetActive(false);
            if (noRomanceVisual) noRomanceVisual.SetActive(false);
            if (laptopStudyVisual) laptopStudyVisual.SetActive(false);

            if (dialogBoxObject) dialogBoxObject.SetActive(false);
            ResetChoicePanel();

            if (blackOverlay != null) 
            {
                blackOverlay.gameObject.SetActive(true);
                blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, 1f);
            }

            if (weekTitleText != null)
            {
                weekTitleText.text = "Week 5 - Weekend Coffee Date";
                weekTitleText.gameObject.SetActive(true);
                weekTitleText.color = new Color(weekTitleText.color.r, weekTitleText.color.g, weekTitleText.color.b, 1f);
            }

            currentCsvIndex = 0; 
            StartCoroutine(RunWeek5CoffeeDateSequence());
        }
        else
        {
            StartCoroutine(RunWeek5SoloStudySequence());
        }
    }

    IEnumerator RunWeek5CoffeeDateSequence()
    {
        userTapped = false;
        isTyping = false;

        yield return new WaitForSeconds(1.8f);

        if (weekTitleText != null) weekTitleText.gameObject.SetActive(false);

        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.8f));
            blackOverlay.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(PlayCsvDialogueSequence("SCENE_05_01"));
    }

    // ==========================================
    // SEKUEN SOLO STUDY (PATH DISTANT DIPOTONG MANUAL & GANTI VISUAL LAPTOP)
    // ==========================================
    IEnumerator RunWeek5SoloStudySequence()
    {
        userTapped = false;
        isTyping = false;

        if (bgCafe) bgCafe.SetActive(false);
        if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(false);
        if (optionASagara) optionASagara.SetActive(false);
        if (optionB1) optionB1.SetActive(false);
        if (optionB2) optionB2.SetActive(false);
        
        // Tampilkan visual awal Solo Study (Kirana di kamar/sendiri)
        if (noRomanceVisual) noRomanceVisual.SetActive(true);
        if (laptopStudyVisual) laptopStudyVisual.SetActive(false);

        if (dialogBoxObject) dialogBoxObject.SetActive(false);
        ResetChoicePanel();

        // Setup Overlay Hitam & Judul Interlude
        if (blackOverlay != null) 
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.color = new Color(blackOverlay.color.r, blackOverlay.color.g, blackOverlay.color.b, 1f);
        }

        if (weekTitleText != null)
        {
            weekTitleText.text = "Week 5 - Solo Study Day";
            weekTitleText.gameObject.SetActive(true);
            weekTitleText.color = new Color(weekTitleText.color.r, weekTitleText.color.g, weekTitleText.color.b, 1f);
        }

        yield return new WaitForSeconds(1.8f);

        if (weekTitleText != null) weekTitleText.gameObject.SetActive(false);

        // Fade-out layar hitam judul
        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, 0.8f));
            blackOverlay.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        // --- DIALOG BAGIAN 1 (Visual Awal Kirana) ---
        yield return StartCoroutine(PlayDialog("Kirana", "Udah saatnya aku ngebales semua nilai jelek aku dengan nilai yang lebih bagus untuk yang selanjutnya."));

        // --- GANTI VISUAL KE BACKGROUND LAPTOP ---
        if (noRomanceVisual) noRomanceVisual.SetActive(false);
        if (laptopStudyVisual) laptopStudyVisual.SetActive(true);

        // --- DIALOG BAGIAN 2 (Visual Laptop) ---
        yield return StartCoroutine(PlayDialog("Kirana", "Mending weekend ini aku fokus belajar aja daripada pergi ke luar."));

        if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
    }

    IEnumerator PlayCsvDialogueSequence(string targetSceneID)
    {
        while (currentCsvIndex < csvRows.Count)
        {
            DialogueRow row = csvRows[currentCsvIndex];

            if (row.sceneID != targetSceneID)
            {
                currentCsvIndex++;
                continue;
            }

            HandleTriggerAction(row.triggerAction);

            if (row.nodeType == "DIALOGUE")
            {
                yield return StartCoroutine(PlayDialog(row.speaker, row.dialogueText));
                currentCsvIndex++;
            }
            else if (row.nodeType == "CHOICE")
            {
                if (invisibleTapButton != null) invisibleTapButton.gameObject.SetActive(false);
                if (dialogBoxObject != null) dialogBoxObject.SetActive(false);
                
                ShowChoicesWeek5();
                yield break; 
            }

            if (row.nextSceneID == "END")
            {
                break;
            }
        }
    }

    void HandleTriggerAction(string action)
    {
        if (string.IsNullOrEmpty(action) || action == "NONE") return;

        if (action == "SHOW_CAFE_SCENE")
        {
            if (bgCafe) bgCafe.SetActive(true);
            if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(true);
        }
        else if (action == "SHOW_ROMANCE_VISUAL")
        {
            if (bgCafe) bgCafe.SetActive(false);
            if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(false);
            if (optionASagara) optionASagara.SetActive(true);
        }
        else if (action == "SHOW_FRIENDSHIP_VISUAL_1")
        {
            if (bgCafe) bgCafe.SetActive(false);
            if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(false);
            if (optionB1) optionB1.SetActive(true);
        }
        else if (action == "SHOW_FRIENDSHIP_VISUAL_2")
        {
            if (optionB1) optionB1.SetActive(false);
            if (optionB2) optionB2.SetActive(true);
        }
        else if (action == "SHOW_SOLO_STUDY")
        {
            if (bgCafe) bgCafe.SetActive(false);
            if (kiranaSagaraVisual) kiranaSagaraVisual.SetActive(false);
            if (noRomanceVisual) noRomanceVisual.SetActive(true);
        }
    }

    private void ShowChoicesWeek5()
    {
        SetButtonText(buttonAText, "A. Buka diri lebih dalam");
        SetButtonText(buttonBText, "B. Fokus diskusi tips belajar");

        buttonAText.onClick.RemoveAllListeners();
        buttonAText.onClick.AddListener(OnSelectOptionA);

        buttonBText.onClick.RemoveAllListeners();
        buttonBText.onClick.AddListener(OnSelectOptionB);

        if (choicePanelWeek5 != null)
        {
            choicePanelWeek5.transform.SetAsLastSibling();
            choicePanelWeek5.gameObject.SetActive(true);
            choicePanelWeek5.alpha = 1f;
            choicePanelWeek5.interactable = true;
            choicePanelWeek5.blocksRaycasts = true;
        }

        if (overlayBlur != null) overlayBlur.SetActive(true); 
    }

    public void OnSelectOptionA()
    {
        ResetChoicePanel();
        FindAndJumpToScene("CHOICE_05_01A");
        StopAllCoroutines();
        StartCoroutine(PlayCsvDialogueSequence("CHOICE_05_01A"));
    }

    public void OnSelectOptionB()
    {
        ResetChoicePanel();
        FindAndJumpToScene("CHOICE_05_01B");
        StopAllCoroutines();
        StartCoroutine(PlayCsvDialogueSequence("CHOICE_05_01B"));
    }

    void FindAndJumpToScene(string sceneID)
    {
        for (int i = 0; i < csvRows.Count; i++)
        {
            if (csvRows[i].sceneID == sceneID)
            {
                currentCsvIndex = i;
                break;
            }
        }
    }

    private void SetButtonText(Button btn, string textStr)
    {
        if (btn != null)
        {
            TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = textStr;
        }
    }

    private void ResetChoicePanel()
    {
        if (choicePanelWeek5 != null)
        {
            choicePanelWeek5.alpha = 0f;
            choicePanelWeek5.gameObject.SetActive(false);
            choicePanelWeek5.interactable = false;
            choicePanelWeek5.blocksRaycasts = false;
        }
        if (overlayBlur != null) overlayBlur.SetActive(false);
    }

    IEnumerator PlayDialog(string speaker, string text)
    {
        if (invisibleTapButton != null) 
        {
            invisibleTapButton.gameObject.SetActive(true);
            invisibleTapButton.transform.SetAsLastSibling();
        }

        if (dialogBoxObject != null)
        {
            if (dialogBoxImageComponent != null)
            {
                if (speaker == "Sagara" && dialogBoxSagaraSprite != null)
                {
                    dialogBoxImageComponent.sprite = dialogBoxSagaraSprite;
                }
                else if (speaker == "Kirana" && dialogBoxKiranaSprite != null)
                {
                    dialogBoxImageComponent.sprite = dialogBoxKiranaSprite;
                }
            }

            dialogBoxObject.transform.SetAsLastSibling();
            dialogBoxObject.SetActive(true);
            CanvasGroup cg = dialogBoxObject.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
        }

        if (dialogueText != null) dialogueText.text = "";

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
}