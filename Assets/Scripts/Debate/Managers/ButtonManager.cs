using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DebateSystem;

namespace DebateSystem
{
    /// <summary>
    /// 统一的按钮点击事件管理器。
    /// 负责接收场景中所有通用按钮的点击，并分发给相应的逻辑控制器。
    /// </summary>
    public class ButtonManager : MonoBehaviour
    {

        #region 公共方法

        /// <summary>
        /// 统一的点击事件处理入口。
        /// 供 EventTrigger 或 Button 组件调用。
        /// </summary>
        /// <param name="btnObj">被点击的按钮对象（可选，如果不传则尝试通过 EventSystem 获取）</param>
        public void OnClickButton(GameObject btnObj = null)
        {
            GameObject targetBtn = btnObj;

            if (targetBtn == null && UnityEngine.EventSystems.EventSystem.current != null)
            {
                targetBtn = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            }

            if (targetBtn == null)
            {
                Debug.LogWarning("[ButtonManager] 无法识别被点击的按钮对象！请检查 EventTrigger 或 Button 绑定。");
                return;
            }

            string btnName = targetBtn.name;
            Debug.Log($"点击了按钮: {btnName}");

            switch (btnName)
            {
                case "AutoButton": // 自动播放按钮
                    if (TreatmentManager.Instance)
                    {
                        // TreatmentManager.Instance.ToggleAutoPlay(); // Removed
                        if (DebateUIManager.Instance)
                        {
                            // DebateUIManager.Instance.UpdateAutoButtonState(targetBtn, TreatmentManager.Instance.isAutoPlay); // Removed
                        }
                    }
                    break;

                case "DialogButton": // 对话框全屏点击按钮 (透明遮罩)
                    if (TreatmentManager.Instance && TreatmentManager.Instance.isWaitingForInput)
                    {
                        if (DebateUIManager.Instance && DebateUIManager.Instance.IsTyping)
                        {
                            DebateUIManager.Instance.CompleteTyping(TreatmentManager.Instance.CurrentLineText);
                        }
                        else
                        {
                            TreatmentManager.Instance.isWaitingForInput = false;
                            TreatmentManager.Instance.NextLine();
                        }
                    }
                    break;

                case "MenuButton": // 菜单按钮
                    if (DebateUIManager.Instance)
                    {
                        DebateUIManager.Instance.ToggleMenuPanel();
                    }
                    break;

                default:
                    Debug.LogWarning($"未处理的按钮点击: {btnName}");
                    break;
            }
        }

        #endregion

        #region 辅助方法

        // UpdateAutoButtonState moved to UIManager

        #endregion
    }
}
