using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包退出按钮：点击后禁用指定物体（通常是背包 UI 根节点）
/// </summary>
[RequireComponent(typeof(Button))]
public class BackpackExitButton : MonoBehaviour
{
    [Header("要禁用的物体")]
    [Tooltip("点击按钮后将被 SetActive(false) 的物体")]
    public GameObject targetToDisable;
    [Tooltip("可选：需要同时关闭的另一个物体，不填则只关闭上面那个")]
    public GameObject ExitToDisable;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnExitClick);
    }

    private void OnExitClick()
    {
        DisableTarget();
    }

    /// <summary>
    /// 禁用目标物体（可供外部调用）
    /// </summary>
    public void DisableTarget()
    {
        if (targetToDisable == null)
        {
            Debug.LogWarning("[BackpackExitButton] 未设置 targetToDisable");
            return;
        }

        targetToDisable.SetActive(false);
        if (ExitToDisable != null)
            ExitToDisable.SetActive(false);
    }
}
