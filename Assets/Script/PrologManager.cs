using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PrologManager : MonoBehaviour
{
    [Header("CSV Settings")]
    public string csvFileName = "WEEK_1_SCRIPT";

    [Header("UI Canvas Elements")]
    public Image blackOverlay;
    public Image backgroundImage;
    public GameObject hpSingleNotif;
    public GameObject hpHokNotif17;
    public GameObject statusPanel;

    [Header("Dialogue System")]
    public GameObject dialogBox;             
    public TMP_Text dialogueText;            
    public TMP_Text speakerText;             
    public Button dialogueButton;            
    public CanvasGroup dialogueCanvasGroup;  

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.04f;
    public float dialogueFadeDuration = 0.2f;

    [Header("Title Card Interlude")]
    public TMP_Text week1TitleText;          // Drag 'Week1Title' (child dari BlackOverlay)

    [Header("Section Parent")]
    public GameObject prologSection;
    public Week1Manager week1Manager;

    [Header("Animation Settings")]
    public float initialFadeDuration = 1.5f;
    public float smoothFadeDuration = 2.0f;
    public float shakeDuration = 0.4f;
    public float shakeMagnitude = 15f;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip alarmSFX;
    public AudioClip beepSFX;

    private List<Dictionary<string, string>> csvData;
    private bool isTyping = false;
    private bool dialogueFinished = false;
    private bool canTapDialogue = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;

    private void Start()
    {
        csvData = CSVReader.Read(csvFileName);

        if (statusPanel != null) statusPanel.SetActive(false);
        if (week1TitleText != null) week1TitleText.gameObject.SetActive(false);
        if (dialogBox != null) dialogBox.SetActive(false);
        if (hpSingleNotif != null) hpSingleNotif.SetActive(false);
        if (hpHokNotif17 != null) hpHokNotif17.SetActive(false);

        if (week1Manager != null && week1Manager.week1Section != null)
        {
            week1Manager.week1Section.SetActive(false);
        }

        if (dialogueButton != null)
        {
            dialogueButton.onClick.RemoveAllListeners();
            dialogueButton.onClick.AddListener(OnDialogueTap);
            dialogueButton.gameObject.SetActive(true);
        }

        StartCoroutine(RunPrologFromCSV());
    }

    // =========================================================
    // MAIN PROLOG SEQUENCE
    // =========================================================

    IEnumerator RunPrologFromCSV()
    {
        // 1. Layar Hitam Awal
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.color = new Color(0f, 0f, 0f, 1f);
            blackOverlay.raycastTarget = false;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Play Percakapan Prolog
        yield return StartCoroutine(PlayScene("SCENE_00_01"));

        // 3. Sembunyikan Dialog Box & Matikan Tap Prolog
        yield return StartCoroutine(HideDialogBoxUtuh());
        if (hpHokNotif17 != null) hpHokNotif17.SetActive(false);
        if (dialogueButton != null) dialogueButton.gameObject.SetActive(false);

        // 4. ANIMASI INTERLUDE / TITLE CARD WEEK 1 🎬
        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
        }

        // Fade Layar ke Hitam
        yield return StartCoroutine(FadeBlack(0f, 1f, smoothFadeDuration));

        // Munculkan Teks 'Week 1 - Do Not Disturb'
        if (week1TitleText != null) 
            week1TitleText.gameObject.SetActive(true);

        // Tahan Teks Title di Layar Hitam selama 2.5 Detik
        yield return new WaitForSeconds(2.5f);

        // Sembunyikan Teks Title
        if (week1TitleText != null) 
            week1TitleText.gameObject.SetActive(false);

        // 5. PINDAH KE WEEK 1
        if (prologSection != null) prologSection.SetActive(false);

        if (week1Manager != null)
        {
            week1Manager.StartWeek1();
        }

        // Fade In dari Hitam ke Gameplay Week 1
        yield return StartCoroutine(FadeBlack(1f, 0f, 1.5f));

        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        if (statusPanel != null) statusPanel.SetActive(true);
    }

    IEnumerator PlayScene(string targetSceneID)
    {
        for (int i = 0; i < csvData.Count; i++)
        {
            string currentScene = csvData[i]["SceneID"];

            if (currentScene == targetSceneID)
            {
                string nodeType = csvData[i]["NodeType"];
                string speaker = csvData[i]["Speaker"];
                string text = csvData[i]["DialogueText"];
                string trigger = csvData[i]["TriggerAction"];
                string sfx = csvData[i]["SFX"];

                if (sfx == "alarm" && sfxSource != null && alarmSFX != null)
                    sfxSource.PlayOneShot(alarmSFX);
                else if (sfx == "beep" && sfxSource != null && beepSFX != null)
                    sfxSource.PlayOneShot(beepSFX);

                if (trigger == "FADE_IN_KAMAR")
                {
                    yield return StartCoroutine(FadeBlack(1f, 0f, initialFadeDuration));
                    if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
                    yield return new WaitForSeconds(2.0f);
                }
                else if (trigger == "SHAKE_HP")
                {
                    yield return StartCoroutine(HideDialogBoxUtuh());
                    if (hpHokNotif17 != null)
                    {
                        hpHokNotif17.SetActive(true);
                        yield return StartCoroutine(ShakeHP(hpHokNotif17.transform, shakeDuration, shakeMagnitude));
                    }
                    yield return new WaitForSeconds(0.8f);
                }

                if (nodeType == "DIALOGUE" && !string.IsNullOrEmpty(text))
                {
                    if (speakerText != null) speakerText.text = speaker;
                    yield return StartCoroutine(ShowDialogue(text));
                }
            }
        }
    }

    IEnumerator ShowDialogue(string text)
    {
        dialogueFinished = false;
        isTyping = false;
        canTapDialogue = true;
        currentFullText = text;

        if (dialogueButton != null && !dialogueButton.gameObject.activeSelf)
            dialogueButton.gameObject.SetActive(true);

        if (dialogBox != null && !dialogBox.activeSelf)
        {
            dialogBox.SetActive(true);
            if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
        }

        if (dialogueCanvasGroup != null && dialogueCanvasGroup.alpha < 1f)
        {
            float timer = 0f;
            while (timer < dialogueFadeDuration)
            {
                timer += Time.deltaTime;
                dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / dialogueFadeDuration);
                yield return null;
            }
            dialogueCanvasGroup.alpha = 1f;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(text));

        yield return new WaitUntil(() => dialogueFinished);

        canTapDialogue = false;
        dialogueFinished = false;
    }

    IEnumerator HideDialogBoxUtuh()
    {
        if (dialogueCanvasGroup != null && dialogBox != null && dialogBox.activeSelf)
        {
            float timer = 0f;
            while (timer < dialogueFadeDuration)
            {
                timer += Time.deltaTime;
                dialogueCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / dialogueFadeDuration);
                yield return null;
            }
            dialogueCanvasGroup.alpha = 0f;
        }

        if (dialogBox != null) dialogBox.SetActive(false);
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void OnDialogueTap()
    {
        if (!canTapDialogue) return;

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullText;
            isTyping = false;
            return;
        }

        if (!isTyping) dialogueFinished = true;
    }

    IEnumerator FadeBlack(float startAlpha, float endAlpha, float duration)
    {
        if (blackOverlay == null) yield break;

        blackOverlay.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            blackOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        blackOverlay.color = new Color(0f, 0f, 0f, endAlpha);
    }

    IEnumerator ShakeHP(Transform hpTransform, float duration, float magnitude)
    {
        Vector3 originalPos = hpTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            hpTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        hpTransform.localPosition = originalPos;
    }
}