using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject dialogueBox;
    public TMP_Text speakerNameText;          // ⭐ 新增：说话者名字（单独的Text）
    public TMP_Text dialogueText;
    public Image characterPortrait;

    [Header("对话内容")]
    public DialogueLine[] dialogueLines;

    [Header("选项相关")]
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;

    [Header("设置")]
    public float typingSpeed = 0.05f;
    public bool autoStart = true;
    public bool dialogueFinished = false;
    public static bool isInDialogue = false;

    public int currentLine = 0;
    public bool isTyping = false;
    public bool canClickNext = false;
    public static DialogueSystem Instance;
    public int clickCount = 0;

    void Awake()
    {
        Instance = this;

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);

        if (speakerNameText != null)
        {
            speakerNameText.text = "";
            speakerNameText.gameObject.SetActive(false);
        }
    }

    public void SetDialogue(DialogueLine[] newLines)
    {
        dialogueLines = newLines;
    }

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);

        if (autoStart)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (UIManager.isUIMode) return;
        if (dialogueBox == null || !dialogueBox.activeSelf) return;

        // ⭐ 排除：对话框以外的 UI（点到按钮等不推进对话；点到对话框本体仍可推进）
        if (IsPointerOverUIExceptDialogueBox())
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTypingImmediate();
            }
            else if (canClickNext)
            {
                clickCount++;
                ProceedToNextLine();
            }
        }
    }

    void SkipTypingImmediate()
    {
        StopAllCoroutines();
        isTyping = false;

        DialogueLine line = dialogueLines[currentLine];

        // ⭐ 名字单独显示
        UpdateSpeakerName(line);

        // 内容直接显示完整文本
        dialogueText.text = line.text;
        SetIconPreserveHeight(characterPortrait, line.portrait);

        // Log（可保留“名字+内容”的格式）
        string displayText = string.IsNullOrEmpty(line.speakerName)
            ? line.text
            : $"<b>[{line.speakerName}]</b>：{line.text}";
        DialogueLogManager.Instance?.AddEntry(displayText);

        if (line.hasChoices && line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
            canClickNext = false;
        }
        else
        {
            canClickNext = true;
        }
    }

    public void StartDialogue()
    {
        isInDialogue = true;
        currentLine = 0;
        clickCount = 0;
        dialogueFinished = false;

        if (dialogueBox != null) dialogueBox.SetActive(true);

        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            foreach (var line in dialogueLines)
            {
                if (line.hasChoices && line.choices != null)
                {
                    foreach (var choice in line.choices)
                        choice.wasChosen = false;
                }
            }
            StartCoroutine(TypeLine(dialogueLines[currentLine]));
        }
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        canClickNext = false;

        // ⭐ 名字单独显示
        UpdateSpeakerName(line);

        dialogueText.text = "";
        SetIconPreserveHeight(characterPortrait, line.portrait);

        foreach (char c in line.text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Log（可保留“名字+内容”的格式）
        string displayText = string.IsNullOrEmpty(line.speakerName)
            ? line.text
            : $"<b>[{line.speakerName}]</b>：{line.text}";
        DialogueLogManager.Instance?.AddEntry(displayText);

        if (line.hasChoices && line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
        else
        {
            canClickNext = true;
        }
    }

    void EndDialogue()
    {
        isInDialogue = false;

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        if (characterPortrait != null) characterPortrait.sprite = null;

        if (speakerNameText != null)
        {
            speakerNameText.text = "";
            speakerNameText.gameObject.SetActive(false);
        }

        dialogueFinished = true;
    }

    void ShowChoices(DialogueChoice[] choices)
{
    canClickNext = false;
    if (choicePanel != null) choicePanel.SetActive(true);

    foreach (Transform child in choicePanel.transform)
        Destroy(child.gameObject);

    foreach (DialogueChoice choice in choices)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);

        var image = btnObj.GetComponent<Image>();
        var button = btnObj.GetComponent<Button>();
        if (image != null) image.enabled = true;
        if (button != null) button.enabled = true;

        // ⭐ 使用 TextMeshProUGUI
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.enabled = true;
            btnText.text = choice.choiceText;
        }

        button.onClick.AddListener(() =>
        {
            choice.wasChosen = true;
            choicePanel.SetActive(false);
            SetDialogue(choice.nextDialogue);
            StartDialogue();
        });
    }
}


    private void SetIconPreserveHeight(Image image, Sprite sprite)
    {
        if (image == null) return;

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        if (sprite == null)
        {
            image.rectTransform.sizeDelta = new Vector2(0, image.rectTransform.sizeDelta.y);
            return;
        }

        float fixedHeight = image.rectTransform.sizeDelta.y;
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
        float newWidth = fixedHeight * aspectRatio;

        Vector2 size = image.rectTransform.sizeDelta;
        size.x = newWidth;
        image.rectTransform.sizeDelta = size;
    }

    public void SkipDialogue()
    {
        if (dialogueBox == null || !dialogueBox.activeSelf || dialogueLines == null || dialogueLines.Length == 0)
            return;

        StopAllCoroutines();

        for (int i = currentLine; i < dialogueLines.Length; i++)
        {
            var line = dialogueLines[i];
            string displayText = string.IsNullOrEmpty(line.speakerName)
                ? line.text
                : $"<b>[{line.speakerName}]</b>：{line.text}";
            DialogueLogManager.Instance?.AddEntry(displayText);
        }

        // 最后一行直接显示（名字也更新）
        DialogueLine last = dialogueLines[dialogueLines.Length - 1];
        UpdateSpeakerName(last);

        dialogueText.text = last.text;
        SetIconPreserveHeight(characterPortrait, last.portrait);

        currentLine = dialogueLines.Length;
        EndDialogue();
    }

    public void SkipTyping()
    {
        if (!isTyping || currentLine >= dialogueLines.Length) return;

        StopAllCoroutines();
        isTyping = false;

        DialogueLine line = dialogueLines[currentLine];

        // ⭐ 名字单独显示
        UpdateSpeakerName(line);

        // 内容只显示正文
        dialogueText.text = line.text;

        // Log（可保留“名字+内容”的格式）
        string displayText = string.IsNullOrEmpty(line.speakerName)
            ? line.text
            : $"<b>[{line.speakerName}]</b>：{line.text}";
        DialogueLogManager.Instance?.AddEntry(displayText);

        canClickNext = true;
    }

    public void ProceedToNextLine()
    {
        if (isTyping) return;

        currentLine++;
        if (currentLine < dialogueLines.Length)
            StartCoroutine(TypeLine(dialogueLines[currentLine]));
        else
            EndDialogue();
    }

    // ⭐ UI 点击过滤：鼠标在“对话框以外的UI”上时，阻止推进
    bool IsPointerOverUIExceptDialogueBox()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // 放行：对话框本体或其子物体（包含名字Text、头像、内容Text等）
            if (dialogueBox != null && result.gameObject.transform.IsChildOf(dialogueBox.transform))
                continue;

            // 点到了其他 UI
            return true;
        }

        return false;
    }

    // ⭐ 统一处理名字显示/隐藏
    void UpdateSpeakerName(DialogueLine line)
    {
        if (speakerNameText == null) return;

        bool hasName = !string.IsNullOrEmpty(line.speakerName);
        speakerNameText.gameObject.SetActive(hasName);
        speakerNameText.text = hasName ? line.speakerName : "";
    }

    public bool IsChoiceChosen(DialogueLine[] lines, int lineIndex, int choiceIndex)
    {
        // 安全检查（数组下标是从0开始的）
        if (lines == null) return false;
        if (lineIndex < 0 || lineIndex >= lines.Length) return false;

        var line = lines[lineIndex];
        if (!line.hasChoices || line.choices == null) return false;
        if (choiceIndex < 0 || choiceIndex >= line.choices.Length) return false;

        return line.choices[choiceIndex].wasChosen;
    }
}
