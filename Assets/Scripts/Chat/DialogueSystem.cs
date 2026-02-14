using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public Image bigcharacterPortrait;
    public Image backgroundImage;             // 新增：背景图片引用

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
    public string dialogueID;
    public bool triggerOnlyOnce;
    public bool loadMainSceneOnEnd = false; // 新增：对话结束后是否返回主界面

    public int currentLine = 0;
    public bool isTyping = false;
    public bool canClickNext = false;
    public static DialogueSystem Instance;
    public int clickCount = 0;

    // ⭐ 新增：对话结束时的回调（用于链式触发事件）
    public System.Action onDialogueEndCallback;

    void Awake()
    {
        Instance = this;

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false); // 初始化隐藏背景

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

        // ⭐ 立即发放线索（如果有）
        if (line.giveEvidence && BackpackManager.Instance != null)
        {
             BackpackManager.Instance.AddEvidence(line.evidenceId, line.evidenceTitle, line.evidenceDesc, line.evidenceIcon);
        }

        // ⭐ 小头像+大头像都更新
        SetPortraits(line);

        // ⭐ 更新背景
        UpdateBackground(line);

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

        // ⭐ 1. 检查是否需要黑屏转场
        if (line.triggerFadeEffect && FadeController.Instance != null)
        {
            // 淡出（变黑）
            yield return StartCoroutine(FadeController.Instance.FadeOut());

            // ⭐ 2. 在全黑状态下更新所有 UI（名字、头像、背景、清空文本）
            UpdateSpeakerName(line);
            dialogueText.text = "";
            SetPortraits(line);
            UpdateBackground(line);

            // 停顿一小会儿（可选，增加节奏感）
            yield return new WaitForSeconds(0.2f);

            // 淡入（变亮）
            yield return StartCoroutine(FadeController.Instance.FadeIn());
        }
        else
        {
            // ⭐ 不需要转场，直接更新 UI
            UpdateSpeakerName(line);
            dialogueText.text = "";
            SetPortraits(line);
            UpdateBackground(line);
        }

        // ⭐ 3. 开始打字
        foreach (char c in line.text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // ⭐ 4. 如果配置了“获得线索”，在打字结束后自动发放
        if (line.giveEvidence && BackpackManager.Instance != null)
        {
            // 避免重复获得（可选，看背包逻辑是否已去重）
            BackpackManager.Instance.AddEvidence(line.evidenceId, line.evidenceTitle, line.evidenceDesc, line.evidenceIcon);
            Debug.Log($"[DialogueSystem] 随对话获得线索：{line.evidenceTitle}");
            
            // 可选：弹出一个临时的小提示（Toast）告诉玩家获得了物品
            // UIManager.Instance.ShowToast($"获得了线索：{line.evidenceTitle}");
        }

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

        // ⭐ 小头像+大头像都清空
        if (characterPortrait != null) 
        {
            characterPortrait.sprite = null;
            characterPortrait.enabled = false; // 隐藏以避免显示白色方块
        }
        if (bigcharacterPortrait != null) 
        {
            bigcharacterPortrait.sprite = null;
            bigcharacterPortrait.enabled = false; // 隐藏以避免显示白色方块
        }

        if (speakerNameText != null)
        {
            speakerNameText.text = "";
            speakerNameText.gameObject.SetActive(false);
        }

        // ⭐ 修改：结束对话时不再强制隐藏背景，保留最后一帧的背景图
        // if (backgroundImage != null) backgroundImage.gameObject.SetActive(false); 

        dialogueFinished = true;

        // ⭐ 触发回调（如果存在）
        if (onDialogueEndCallback != null)
        {
            onDialogueEndCallback.Invoke();
        }

        // ⭐ 如果设置了结束后返回主界面
        if (loadMainSceneOnEnd)
        {
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// 外部接口：设置对话结束后是否返回主界面
    /// </summary>
    public void SetReturnToMainOnEnd(bool shouldReturn)
    {
        loadMainSceneOnEnd = shouldReturn;
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

    // ⭐ 新增：同时设置小头像和大头像
    void SetPortraits(DialogueLine line)
    {
        // 小头像：不改变尺寸，仅设置图片（用户已在场景调整好宽高）
        SetIconSimple(characterPortrait, line.portrait);

        // 大头像：保持原有逻辑（高度固定，宽度自适应）
        SetIconPreserveHeight(bigcharacterPortrait, line.portrait);
    }

    // ⭐ 更新背景图片
    void UpdateBackground(DialogueLine line)
    {
        if (backgroundImage == null) return;

        if (line.background != null)
        {
            backgroundImage.gameObject.SetActive(true);
            backgroundImage.sprite = line.background;
        }
        else
        {
            // 如果当前句没有背景，是否需要隐藏？
            // 通常有两种设计：
            // 1. 隐藏背景（如下）
            // 2. 保持上一句的背景（如果需要保持，就注释掉下面的 SetActive(false)）
            //backgroundImage.gameObject.SetActive(false);
        }
    }

    // 简单设置图片，不修改 RectTransform
    private void SetIconSimple(Image image, Sprite sprite)
    {
        if (image == null) return;

        image.sprite = sprite;
        // 如果没有图片，则禁用 Image 组件以隐藏；否则启用
        image.enabled = (sprite != null);
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
            image.enabled = false;
            return;
        }
        image.enabled = true;

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

        // ⭐ 小头像+大头像都更新
        SetIconSimple(characterPortrait, last.portrait);
        SetIconPreserveHeight(bigcharacterPortrait, last.portrait);

        // ⭐ 更新背景
        UpdateBackground(last);

        // ⭐ 检查最后一句是否有线索（跳过打字也得给）
        if (last.giveEvidence && BackpackManager.Instance != null)
        {
             BackpackManager.Instance.AddEvidence(last.evidenceId, last.evidenceTitle, last.evidenceDesc, last.evidenceIcon);
        }

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

        // ⭐ 可选但推荐：跳过打字时头像也同步更新
        SetPortraits(line);

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
