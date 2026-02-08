using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 背包界面右上角退出按钮。
/// - 预制体模式：作为面板在 UI 场景内时，勾选 closeAsPanel 会关闭背包面板返回主页面。
/// - 场景模式：未勾选时加载 targetSceneName 场景。
/// </summary>
[RequireComponent(typeof(Button))]
public class BackpackExitButton : MonoBehaviour
{
    [Tooltip("勾选：作为面板使用时关闭背包并返回主页面；不勾选：加载目标场景（用于独立背包场景）")]
    public bool closeAsPanel = true;

    [Tooltip("未使用 closeAsPanel 时加载的场景名称")]
    public string targetSceneName = "UI";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnExitClick);
    }

    private void OnExitClick()
    {
        if (closeAsPanel && BackpackUI.Instance != null)
            BackpackUI.Instance.CloseBackpack();
        else
            ExitToScene();
    }

    /// <summary>加载目标场景</summary>
    public void ExitToScene()
    {
        if (FadeController.Instance != null)
            FadeController.Instance.FadeAndLoadScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }
}
