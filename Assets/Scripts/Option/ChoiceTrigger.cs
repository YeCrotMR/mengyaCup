using System.Collections.Generic;
using UnityEngine;

public class ChoiceTriggerMulti : MonoBehaviour
{
    [System.Serializable]
    public class ChoiceOption
    {
        [TextArea]
        public string content;

        public bool lockAfterChoose;
    }

    [Header("Trigger ID")]
    public string triggerId;

    [Header("选项设置")]
    public List<ChoiceOption> options = new List<ChoiceOption>();

    private bool locked = false;
    private ChoiceSystemMulti choiceSystem;

    void Awake()
    {
        // 自动寻找场景中的 ChoiceSystemMulti
        choiceSystem = FindObjectOfType<ChoiceSystemMulti>();

        if (choiceSystem == null)
        {
            Debug.LogError("场景中没有找到 ChoiceSystemMulti！");
        }
    }

    private void OnMouseDown()
    {
        if (locked) return;
        if (choiceSystem == null) return;

        List<string> optionTexts = new List<string>();
        foreach (var opt in options)
        {
            optionTexts.Add(opt.content);
        }

        choiceSystem.ActivateChoice(optionTexts);

        choiceSystem.OnChoiceMade = (index) =>
        {
            Debug.Log($"TriggerID: {triggerId}  选择序号: {index}");

            // 返回 ID + 序号
            HandleChoice(triggerId, index);

            if (index >= 0 && index < options.Count)
            {
                if (options[index].lockAfterChoose)
                {
                    locked = true;
                    Debug.Log("选择了锁定选项，已无法再次触发");
                }
            }
        };
    }

    private void HandleChoice(string id, int index)
    {
        Debug.Log($"收到选择 -> ID: {id}, Index: {index}");
    }
}
