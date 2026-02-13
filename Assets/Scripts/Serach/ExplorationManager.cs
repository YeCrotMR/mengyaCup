using UnityEngine;
using System.Collections.Generic;

public class ExplorationManager : MonoBehaviour
{
    [Header("配置：所有需要调查的物品父节点")]
    public Transform itemsParent;

    [Header("配置：探索结束后的剧情对话")]
    public DialogueLine[] endExplorationDialogue;

    [Header("配置：下一个场景的名字")]
    public string nextSceneName;

    [Header("配置：检查频率(秒)")]
    public float checkInterval = 1f;

    private List<SearchableItem> allItems = new List<SearchableItem>();
    private bool isFinished = false;

    private void Start()
    {
        // 1. 自动收集父节点下所有的 SearchableItem
        if (itemsParent != null)
        {
            allItems.AddRange(itemsParent.GetComponentsInChildren<SearchableItem>());
        }
        else
        {
            // 如果没配父节点，就尝试全场景搜（可选，或者报错）
            Debug.LogWarning("ExplorationManager: 未配置 Items Parent，尝试查找场景中所有 SearchableItem...");
            allItems.AddRange(FindObjectsOfType<SearchableItem>());
        }

        // 2. 启动定期检查
        InvokeRepeating(nameof(CheckAllItems), 1f, checkInterval);
    }

    private void CheckAllItems()
    {
        if (isFinished) return;
        if (DialogueSystem.isInDialogue) return; // 对话中不打断

        // 排除掉所有未激活（Inactive）的物品，只检查当前激活的物品
        // 这样如果有些物品被隐藏了（SetActive(false)），就不会卡住流程
        // 同时强制要求所有 item.oneTimeOnly 必须为 true 且 IsUsed 为 true 才算完成
        // 或者简单点：检查 allItems 中所有 active 的物品，是否都 IsUsed 了

        bool allFound = true;
        foreach (var item in allItems)
        {
            // 如果物体本身被禁用/隐藏了，跳过检查（视为不需要调查）
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            // 必须是“一次性”物品才算任务目标
            if (item.oneTimeOnly)
            {
                // 只要发现有一个没被点过，就说明没找完
                if (!item.IsUsed)
                {
                    allFound = false;
                    break;
                }
            }
        }

        if (allFound)
        {
            FinishExploration();
        }
    }

    private void FinishExploration()
    {
        isFinished = true;
        CancelInvoke(nameof(CheckAllItems));

        Debug.Log("探索完成！启动结束流程...");

        // 1. 设置回调：对话结束后加载场景
        DialogueSystem.Instance.onDialogueEndCallback = () =>
        {
            DialogueSystem.Instance.onDialogueEndCallback = null; // 清理
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                // 直接加载场景，利用新场景 FadeController 的 OnEnable 自动黑屏变亮
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("ExplorationManager: 未配置 NextSceneName");
            }
        };

        // 2. 播放结束对话
        if (endExplorationDialogue != null && endExplorationDialogue.Length > 0)
        {
            DialogueSystem.Instance.SetDialogue(endExplorationDialogue);
            DialogueSystem.Instance.StartDialogue();
        }
        else
        {
            // 如果没配对话，直接切场景
            DialogueSystem.Instance.onDialogueEndCallback?.Invoke();
        }
    }
}
