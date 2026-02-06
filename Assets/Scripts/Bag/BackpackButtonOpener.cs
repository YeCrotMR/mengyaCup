using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂在主页面「背包」按钮上，点击时打开背包界面。
/// 若当前场景有 BackpackUI 则打开面板；若无则加载背包场景。
/// </summary>
[RequireComponent(typeof(Button))]
public class BackpackButtonOpener : MonoBehaviour
{
    [Tooltip("当场景中无 BackpackUI 时，加载的背包场景名称")]
    public string backpackSceneName = "Backpack";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenBackpack);
    }

    public void OpenBackpack()
    {
        if (BackpackUI.Instance != null)
        {
            BackpackUI.Instance.OpenBackpack();
        }
        else
        {
            if (FadeController.Instance != null)
                FadeController.Instance.FadeAndLoadScene(backpackSceneName);
            else
                SceneManager.LoadScene(backpackSceneName);
        }
    }
}
