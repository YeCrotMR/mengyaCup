using UnityEngine;

/// <summary>
/// 找寻系统：探索阶段房间内可点击的互动物品。
/// 点击后三种情况：1. 对话框陈述物品  2. 触发选择事件  3. 获得关键信息（特写图）并记录至背包。
/// </summary>
public class SearchableItem : MonoBehaviour
{
    public enum InteractionType
    {
        /// <summary>在对话框里陈述物品</summary>
        DescribeInDialog,

        /// <summary>触发选择事件（分支选项）</summary>
        TriggerChoice,

        /// <summary>获得关键信息（特写图片）并记录至背包</summary>
        KeyEvidenceToBackpack
    }

    [Header("互动类型")]
    public InteractionType interactionType = InteractionType.DescribeInDialog;

    [Header("陈述物品（类型为 DescribeInDialog 时使用）")]
    [TextArea(2, 4)]
    public string describeText = "这是某种物品的描写……";
    public Sprite describePortrait;

    [Header("选择事件（类型为 TriggerChoice 时使用）")]
    public DialogueLine[] choiceDialogueLines;

    [Header("关键信息入背包（类型为 KeyEvidenceToBackpack 时使用）")]
    public string evidenceId = "evidence_01";
    public string evidenceTitle = "关键证物";
    [TextArea(2, 4)]
    public string evidenceDescription = "证物的详细描述。";
    public Sprite evidenceCloseUpImage;

    [Header("可选：点击后禁用再次点击")]
    public bool oneTimeOnly = false;

    private bool used = false;
    private Collider2D col2d;
    private Collider col3d;

    private void Awake()
    {
        col2d = GetComponent<Collider2D>();
        col3d = GetComponent<Collider>();
    }

    private void OnMouseDown()
    {
        if (oneTimeOnly && used) return;
        if (DialogueSystem.isInDialogue) return;
        if (UIManager.isUIMode) return;

        switch (interactionType)
        {
            case InteractionType.DescribeInDialog:
                RunDescribe();
                break;
            case InteractionType.TriggerChoice:
                RunChoice();
                break;
            case InteractionType.KeyEvidenceToBackpack:
                RunKeyEvidence();
                break;
        }

        if (oneTimeOnly) used = true;
    }

    private void RunDescribe()
    {
        var line = new DialogueLine
        {
            text = describeText,
            portrait = describePortrait,
            hasChoices = false
        };
        var lines = new DialogueLine[] { line };
        DialogueSystem.Instance.SetDialogue(lines);
        DialogueSystem.Instance.StartDialogue();
    }

    private void RunChoice()
    {
        if (choiceDialogueLines == null || choiceDialogueLines.Length == 0)
        {
            Debug.LogWarning("[SearchableItem] TriggerChoice 未配置 choiceDialogueLines。");
            return;
        }
        DialogueSystem.Instance.SetDialogue(choiceDialogueLines);
        DialogueSystem.Instance.StartDialogue();
    }

    private void RunKeyEvidence()
    {
        if (BackpackManager.Instance != null)
        {
            BackpackManager.Instance.AddEvidence(evidenceId, evidenceTitle, evidenceDescription, evidenceCloseUpImage);
        }

        var line = new DialogueLine
        {
            text = "获得了关键信息：「" + evidenceTitle + "」已记录至背包。",
            portrait = null,
            hasChoices = false
        };
        var lines = new DialogueLine[] { line };
        DialogueSystem.Instance.SetDialogue(lines);
        DialogueSystem.Instance.StartDialogue();
    }
}
