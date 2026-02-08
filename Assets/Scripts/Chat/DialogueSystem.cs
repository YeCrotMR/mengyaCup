using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject dialogueBox;
    public Text dialogueText;
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
        dialogueBox.SetActive(false);
        choicePanel.SetActive(false);
    }

    public void SetDialogue(DialogueLine[] newLines)
    {
        dialogueLines = newLines;
    }

    void Start()
    {
        dialogueBox.SetActive(false);

        if (autoStart)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (UIManager.isUIMode) return;
        if (!dialogueBox.activeSelf) return;

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
        dialogueText.text = line.text;
        SetIconPreserveHeight(characterPortrait, line.portrait);

        DialogueLogManager.Instance?.AddEntry(line.text);

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
        dialogueBox.SetActive(true);

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
        dialogueText.text = "";
        SetIconPreserveHeight(characterPortrait, line.portrait);

        foreach (char c in line.text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

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
        dialogueBox.SetActive(false);
        dialogueText.text = "";
        characterPortrait.sprite = null;
        dialogueFinished = true;

    }

    void ShowChoices(DialogueChoice[] choices)
    {
        canClickNext = false;
        choicePanel.SetActive(true);

        foreach (Transform child in choicePanel.transform)
            Destroy(child.gameObject);

        foreach (DialogueChoice choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);

            var image = btnObj.GetComponent<Image>();
            var button = btnObj.GetComponent<Button>();
            if (image != null) image.enabled = true;
            if (button != null) button.enabled = true;

            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.enabled = true;
                btnText.text = choice.choiceText;

                Shadow shadow = btnText.GetComponent<Shadow>();
                if (shadow != null) shadow.enabled = true;
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
        if (!dialogueBox.activeSelf || dialogueLines == null || dialogueLines.Length == 0)
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

        dialogueText.text = dialogueLines[dialogueLines.Length - 1].text;
        SetIconPreserveHeight(characterPortrait, dialogueLines[dialogueLines.Length - 1].portrait);

        currentLine = dialogueLines.Length;
        EndDialogue();
    }

    public void SkipTyping()
    {
        if (!isTyping || currentLine >= dialogueLines.Length) return;

        StopAllCoroutines();
        isTyping = false;

        DialogueLine line = dialogueLines[currentLine];
        string displayText = string.IsNullOrEmpty(line.speakerName)
            ? line.text
            : $"<b>[{line.speakerName}]</b>：{line.text}";

        dialogueText.text = displayText;
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
