using System.Collections.Generic;
using UnityEngine;

public class ChoiceTrigger : MonoBehaviour
{
    [System.Serializable]
    public class ChoiceOption
    {
        [TextArea]
        public string content;

        [Tooltip("选择该选项后触发的对话（可不填）")]
        public DialogueLine[] nextDialogue;

        [Tooltip("选择后是否锁定，不能再触发")]
        public bool lockAfterChoose;
    }

    [Header("Trigger ID")]
    public string triggerId;

    [Header("选项设置")]
    public List<ChoiceOption> options = new List<ChoiceOption>();

    private bool locked = false;
    private ChoiceSystem choiceSystem;

    void Awake()
    {
        choiceSystem = FindObjectOfType<ChoiceSystem>();

        if (choiceSystem == null)
        {
            Debug.LogError("场景中没有找到 ChoiceSystemMulti！");
        }
    }

    private void OnMouseDown()
    {
        if (locked) return;
        if (choiceSystem == null) return;

        // 1) 把本脚本的选项转换为 ChoiceSystem 的选项数据
        var uiOptions = new List<ChoiceSystem.ChoiceOption>(options.Count);
        foreach (var opt in options)
        {
            uiOptions.Add(new ChoiceSystem.ChoiceOption
            {
                text = opt.content,
                nextDialogue = opt.nextDialogue
            });
        }

        // 2) 激活选项界面
        choiceSystem.ActivateChoice(uiOptions);

        // 3) 临时绑定回调：触发后清空，避免覆盖/串台
        choiceSystem.OnChoiceMade = (index) =>
        {
            Debug.Log($"TriggerID: {triggerId}  选择序号: {index}");

            HandleChoice(triggerId, index);

            // 锁定逻辑
            if (index >= 0 && index < options.Count)
            {
                if (options[index].lockAfterChoose)
                {
                    locked = true;
                    Debug.Log("选择了锁定选项，已无法再次触发");
                }
            }

            // ✅ 清理：避免影响下一个 Trigger
            choiceSystem.OnChoiceMade = null;
        };
    }

    private void HandleChoice(string id, int index)
    {
        Debug.Log($"收到选择 -> ID: {id}, Index: {index}");
    }
}
