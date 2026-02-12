using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceSystemMulti : MonoBehaviour
{
    [Header("生成按钮用")]
    public Transform optionsRoot;          // 按钮生成的父物体（建议挂 VerticalLayoutGroup）
    public Button buttonPrefab;            // 按钮预制体（里面要有 TMP_Text）

    // 本次选择结果（点击按钮时赋值）
    public int? choiceIndex = null;

    // 回调：返回选项序号 0,1,2...
    public Action<int> OnChoiceMade;

    private readonly List<Button> spawnedButtons = new List<Button>();

    void Awake()
    {
        // 默认禁用界面
        //gameObject.SetActive(false);
    }

    /// <summary>
    /// 激活并生成选项按钮
    /// </summary>
    public void ActivateChoice(List<string> options)
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
            var btn = Instantiate(buttonPrefab, optionsRoot);
            spawnedButtons.Add(btn);

            // 设置文本（预制体里找 TMP_Text）
            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = options[i];

            btn.onClick.AddListener(() => Select(index));
        }

        gameObject.SetActive(true);
    }

    private void Select(int index)
    {
        choiceIndex = index;
        OnChoiceMade?.Invoke(index);
        gameObject.SetActive(false);
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
