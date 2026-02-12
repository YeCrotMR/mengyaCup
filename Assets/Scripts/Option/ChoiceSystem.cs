using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceSystem : MonoBehaviour
{
    [Header("生成按钮用")]
    public Transform optionsRoot;          // 按钮生成的父物体（建议挂 VerticalLayoutGroup）
    public Button buttonPrefab;            // 按钮预制体（里面要有 TMP_Text）

    [Header("选择后行为")]
    public bool autoStartDialogueOnSelect = true; // 选完是否自动播放该选项绑定的对话

    // 本次选择结果（点击按钮时赋值）
    public int? choiceIndex = null;

    // 回调：返回选项序号 0,1,2...
    public Action<int> OnChoiceMade;

    private readonly List<Button> spawnedButtons = new List<Button>();

    // ⭐ 新增：一个选项的数据（文本 + 可选的下一段对话）
    [System.Serializable]
    public class ChoiceOption
    {
        public string text;

        [Tooltip("选择该选项后要触发的对话（DialogueLine[]）。为空则不触发。")]
        public DialogueLine[] nextDialogue;
    }



    /// <summary>
    /// 激活并生成选项按钮（新版：每个选项都可以配置 nextDialogue）
    /// </summary>
    public void ActivateChoice(List<ChoiceOption> options)
    {
        choiceIndex = null;
        ClearButtons();

        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("ActivateChoice: options 为空，无法生成选项。");
            return;
        }

        for (int i = 0; i < options.Count; i++)
        {
            int index = i; // 闭包必须拷贝
            ChoiceOption opt = options[i];

            var btn = Instantiate(buttonPrefab, optionsRoot);
            spawnedButtons.Add(btn);

            // 设置文本（预制体里找 TMP_Text）
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = opt != null ? opt.text : "";

            btn.onClick.AddListener(() => Select(index, opt));
        }

        gameObject.SetActive(true);
    }

    // ✅ 兼容你旧的调用方式（只传文本）
    public void ActivateChoice(List<string> options)
    {
        if (options == null) { ActivateChoice((List<ChoiceOption>)null); return; }

        var converted = new List<ChoiceOption>(options.Count);
        for (int i = 0; i < options.Count; i++)
        {
            converted.Add(new ChoiceOption { text = options[i], nextDialogue = null });
        }
        ActivateChoice(converted);
    }

    private void Select(int index, ChoiceOption opt)
    {
        choiceIndex = index;
        OnChoiceMade?.Invoke(index);
        gameObject.SetActive(false);

        // ⭐ 新增：如果该选项配置了对话，就触发对话
        if (autoStartDialogueOnSelect && opt != null && opt.nextDialogue != null && opt.nextDialogue.Length > 0)
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.SetDialogue(opt.nextDialogue);
                DialogueSystem.Instance.StartDialogue();
            }
            else
            {
                Debug.LogWarning("ChoiceSystemMulti: DialogueSystem.Instance 为 null，无法触发对话。");
            }
        }
    }

    private void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null) Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }
}

