using UnityEngine;
using UnityEngine.SceneManagement;

public class backstart : MonoBehaviour
{
    // 设置目标场景名称（在Inspector里填）
    public int index;

    // 按钮点击调用这个方法
    public void ChangeScene()
    {
        FadeController.Instance?.FadeAndLoadScene(index);
    
        Debug.Log("切换场景成功");
    }
}