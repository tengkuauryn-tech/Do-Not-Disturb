using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpotlightTutorial : MonoBehaviour
{
    [Header("Main Container")]
    public GameObject tutorialPanel;
    public RectTransform canvasRect; // Drag Canvas utama ke sini

    [Header("4 Sisi Gelap Pembentuk Lubang Terang")]
    public RectTransform darkTop;
    public RectTransform darkBottom;
    public RectTransform darkLeft;
    public RectTransform darkRight;
    public RectTransform highlightFrame; // Drag HighlightFrame (PNG Glow) ke sini

    [Header("Kotak Penjelasan & Tap")]
    public RectTransform dialogBoxRect;
    public TMP_Text explanationText;
    public Button tapNextButton; // Drag Invisible Tap Area ke sini

    [Header("Target UI yang Akan Disorot (Drag dari Hierarchy)")]
    public RectTransform[] targetStatusItems;   // Size = 5 -> Task, EnergySlider, FocusSlider, StressSlider, RelationshipSlider
    public RectTransform[] targetChoiceButtons; // Size = 3 -> Button_A, Button_B, Button_C
    public RectTransform targetDNDButton;       // Drag ToggleDND_Button

    [Header("Pengaturan Ukuran, Warna & Animasi")]
    [Range(0f, 1f)]
    public float darkAlpha = 0.92f;       // Gelap pekat biar fokus penuh
    public float padding = 10f;           // Jarak lubang dari pinggir target
    public float statusTopExtra = 28f;    // Tambahan tinggi atas khusus indikator status
    public float statusExtraScale = 16f;  // Tambahan lebar/tinggi umum khusus indikator status
    public float dndExtraRight = 300f;    // Tambahan lebar ke kanan buat ngebungkus tulisan "Do Not Disturb"
    public float frameExtraOffset = 18f;  // Lebar cahaya glow luar PNG
    public float moveDuration = 0.35f;    // Durasi pindah antar step
    public float slideCycleDelay = 0.7f;  // Jeda geser antar item (Indikator & Pilihan)
    public float typingSpeed = 0.025f;    // Kecepatan animasi mengetik

    private bool isTapped = false;
    private Rect currentHoleRect;
    private Coroutine activeMoveRoutine;

    void Awake()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public IEnumerator PlayTutorial()
    {
        if (tutorialPanel == null || canvasRect == null) yield break;

        ApplyDarkAlpha();
        tutorialPanel.SetActive(true);
        tutorialPanel.transform.SetAsLastSibling();

        if (tapNextButton != null)
        {
            tapNextButton.gameObject.SetActive(true);
            tapNextButton.interactable = true;
            Image tapImg = tapNextButton.GetComponent<Image>();
            if (tapImg != null) tapImg.raycastTarget = true;

            tapNextButton.transform.SetAsLastSibling();
            tapNextButton.onClick.RemoveAllListeners();
            tapNextButton.onClick.AddListener(() => isTapped = true);
        }

        currentHoleRect = new Rect(0, 0, 100, 100);

        // --- STEP 1: SOROT INDIKATOR STATUS SATU PER SATU (TASK -> ENERGY -> FOCUS -> STRESS -> RELATIONSHIP) ---
        if (targetStatusItems != null && targetStatusItems.Length > 0)
        {
            yield return StartCoroutine(PlaySlidingStep(
                targetStatusItems,
                "INDIKATOR STATUS\nJaga keseimbangan bar di atas ini—setiap keputusanmu memengaruhi kondisi Kirana sepanjang permainan.",
                instantFirst: true,
                extraTop: statusTopExtra,
                extraScale: statusExtraScale
            ));
        }

        // --- STEP 2: SOROT PILIHAN (A -> B -> C) ---
        if (targetChoiceButtons != null && targetChoiceButtons.Length > 0)
        {
            yield return StartCoroutine(PlaySlidingStep(
                targetChoiceButtons,
                "PILIHAN & KONSEKUENSI\nSetiap pilihan menentukan arah cerita, hubungan karakter, hingga ending yang kamu dapat.",
                instantFirst: false
            ));
        }

        // --- STEP 3: SOROT TOMBOL DO NOT DISTURB ---
        if (targetDNDButton != null)
        {
            bool wasActive = targetDNDButton.gameObject.activeSelf;
            targetDNDButton.gameObject.SetActive(true);

            yield return StartCoroutine(MoveSpotlightTo(targetDNDButton, false, extraRight: dndExtraRight));
            yield return StartCoroutine(TypeTextAndWait(
                "MODE DO NOT DISTURB\nBlokir gangguan notifikasi agar bisa fokus. Hanya muncul di momen tertentu—gunakan dengan bijak!"
            ));

            if (!wasActive) targetDNDButton.gameObject.SetActive(false);
        }

        if (tapNextButton != null)
        {
            tapNextButton.onClick.RemoveAllListeners();
            tapNextButton.gameObject.SetActive(false);
        }

        tutorialPanel.SetActive(false);
    }

    private IEnumerator PlaySlidingStep(RectTransform[] items, string message, bool instantFirst, float extraTop = 0f, float extraScale = 0f)
    {
        if (items[0] != null)
        {
            items[0].gameObject.SetActive(true);
            yield return StartCoroutine(MoveSpotlightTo(items[0], instantFirst, extraTop: extraTop, extraScale: extraScale));
        }

        yield return new WaitForSeconds(0.15f);
        isTapped = false;

        int idx = 0;
        float timer = 0f;

        if (explanationText != null)
        {
            explanationText.text = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (isTapped)
                {
                    explanationText.text = message;
                    isTapped = false;
                    yield return new WaitForSeconds(0.15f);
                    break;
                }

                explanationText.text += message[i];

                timer += typingSpeed;
                if (timer >= slideCycleDelay && items.Length > 1)
                {
                    timer = 0f;
                    idx = (idx + 1) % items.Length;
                    TriggerSlideTo(items[idx], extraTop, extraScale);
                }

                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTapped = false;
        while (!isTapped)
        {
            timer += Time.deltaTime;
            if (timer >= slideCycleDelay && items.Length > 1)
            {
                timer = 0f;
                idx = (idx + 1) % items.Length;
                TriggerSlideTo(items[idx], extraTop, extraScale);
            }
            yield return null;
        }
        isTapped = false;
    }

    private void TriggerSlideTo(RectTransform nextTarget, float extraTop = 0f, float extraScale = 0f)
    {
        if (nextTarget == null) return;
        nextTarget.gameObject.SetActive(true);
        if (activeMoveRoutine != null) StopCoroutine(activeMoveRoutine);
        activeMoveRoutine = StartCoroutine(MoveSpotlightTo(nextTarget, false, 0.28f, moveDialog: false, extraTop: extraTop, extraScale: extraScale));
    }

    private IEnumerator TypeTextAndWait(string message)
    {
        yield return new WaitForSeconds(0.15f);
        isTapped = false;

        if (explanationText != null)
        {
            explanationText.text = "";
            for (int i = 0; i < message.Length; i++)
            {
                if (isTapped)
                {
                    explanationText.text = message;
                    isTapped = false;
                    yield return new WaitForSeconds(0.15f);
                    break;
                }
                explanationText.text += message[i];
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTapped = false;
        while (!isTapped) yield return null;
        isTapped = false;
    }

    private void ApplyDarkAlpha()
    {
        RectTransform[] darks = { darkTop, darkBottom, darkLeft, darkRight };
        foreach (var d in darks)
        {
            if (d != null)
            {
                Image img = d.GetComponent<Image>();
                if (img != null) img.color = new Color(0f, 0f, 0f, darkAlpha);
            }
        }
    }

    IEnumerator MoveSpotlightTo(RectTransform target, bool instant, float customDur = -1f, bool moveDialog = true, float extraTop = 0f, float extraScale = 0f, float extraRight = 0f)
    {
        if (target == null) yield break;

        Rect targetRect = GetVisualBoundsInCanvas(target);
        targetRect.xMin -= (padding + extraScale);
        targetRect.xMax += (padding + extraScale + extraRight);
        targetRect.yMin -= (padding + extraScale);
        targetRect.yMax += (padding + extraScale + extraTop);

        if (moveDialog && dialogBoxRect != null)
        {
            if (targetRect.center.x > 120f)
                dialogBoxRect.anchoredPosition = new Vector2(-320f, 0f);
            else if (targetRect.center.y > 0f)
                dialogBoxRect.anchoredPosition = new Vector2(0f, -160f);
            else
                dialogBoxRect.anchoredPosition = new Vector2(0f, 160f);
        }

        float dur = customDur > 0f ? customDur : moveDuration;

        if (instant)
        {
            currentHoleRect = targetRect;
            ApplyHoleRect(currentHoleRect);
            yield break;
        }

        Rect startRect = currentHoleRect;
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);

            currentHoleRect = Rect.MinMaxRect(
                Mathf.Lerp(startRect.xMin, targetRect.xMin, t),
                Mathf.Lerp(startRect.yMin, targetRect.yMin, t),
                Mathf.Lerp(startRect.xMax, targetRect.xMax, t),
                Mathf.Lerp(startRect.yMax, targetRect.yMax, t)
            );

            ApplyHoleRect(currentHoleRect);
            yield return null;
        }

        currentHoleRect = targetRect;
        ApplyHoleRect(currentHoleRect);
    }

    private void ApplyHoleRect(Rect hole)
    {
        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;

        if (darkTop != null)
        {
            float topH = Mathf.Max(0f, halfH - hole.yMax);
            darkTop.anchoredPosition = new Vector2(0f, hole.yMax + topH * 0.5f);
            darkTop.sizeDelta = new Vector2(canvasRect.rect.width, topH);
        }

        if (darkBottom != null)
        {
            float botH = Mathf.Max(0f, hole.yMin - (-halfH));
            darkBottom.anchoredPosition = new Vector2(0f, hole.yMin - botH * 0.5f);
            darkBottom.sizeDelta = new Vector2(canvasRect.rect.width, botH);
        }

        if (darkLeft != null)
        {
            float leftW = Mathf.Max(0f, hole.xMin - (-halfW));
            darkLeft.anchoredPosition = new Vector2(hole.xMin - leftW * 0.5f, hole.center.y);
            darkLeft.sizeDelta = new Vector2(leftW, hole.height);
        }

        if (darkRight != null)
        {
            float rightW = Mathf.Max(0f, halfW - hole.xMax);
            darkRight.anchoredPosition = new Vector2(hole.xMax + rightW * 0.5f, hole.center.y);
            darkRight.sizeDelta = new Vector2(rightW, hole.height);
        }

        if (highlightFrame != null)
        {
            highlightFrame.anchoredPosition = hole.center;
            highlightFrame.sizeDelta = new Vector2(
                hole.width + (frameExtraOffset * 2f),
                hole.height + (frameExtraOffset * 2f)
            );
        }
    }

    private Rect GetVisualBoundsInCanvas(RectTransform root)
    {
        Vector3[] corners = new Vector3[4];
        root.GetWorldCorners(corners);

        Vector2 min = canvasRect.InverseTransformPoint(corners[0]);
        Vector2 max = canvasRect.InverseTransformPoint(corners[2]);

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
}