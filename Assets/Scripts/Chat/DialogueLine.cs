using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string text;                     // 对话文本
    public Sprite portrait;                 // 对应立绘图像

    public bool hasChoices = false;
    public DialogueChoice[] choices;
    public string speakerName;
    public Sprite background;               // 新增：背景图片
    public bool triggerFadeEffect = false;  // 新增：是否触发黑屏转场

    [Header("获得线索（可选）")]
    public bool giveEvidence = false;
    public string evidenceId;
    public string evidenceTitle;
    [TextArea] public string evidenceDesc;
    public Sprite evidenceIcon;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueLine[] nextDialogue;
    [HideInInspector] public bool wasChosen = false;
}





