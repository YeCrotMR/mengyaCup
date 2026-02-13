using UnityEngine;
using UnityEngine.UI;

namespace DebateSystem
{
    /// <summary>
    /// 控制 UI 图片进行平滑的透明度闪烁效果。
    /// 适用于输入指示图标（Waiting Signal）。
    /// </summary>
    public class UIAlphaBlink : MonoBehaviour
    {
        #region 变量定义

        [Header("Settings")]
        [Tooltip("闪烁周期（秒），即从最亮到最暗再回到最亮的时间")]
        public float blinkDuration = 1.0f;

        [Tooltip("最小透明度 (0-1)")]
        [Range(0f, 1f)]
        public float minAlpha = 0.2f;

        [Tooltip("最大透明度 (0-1)")]
        [Range(0f, 1f)]
        public float maxAlpha = 1.0f;

        private Image targetImage;
        private CanvasGroup targetCanvasGroup;
        private float timer;

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            targetImage = GetComponent<Image>();
            
            if (targetImage == null)
            {
                targetCanvasGroup = GetComponent<CanvasGroup>();
            }

            if (targetImage == null && targetCanvasGroup == null)
            {
                Debug.LogWarning($"[UIAlphaBlink] {gameObject.name} 上未找到 Image 或 CanvasGroup 组件，脚本将失效。");
                enabled = false;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // 使用 PingPong 函数产生 0 到 blinkDuration 的往复值
            // t 会在 0 到 1 之间平滑变化
            float t = Mathf.PingPong(timer, blinkDuration) / blinkDuration;

            // 插值计算当前透明度
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            // 应用透明度
            if (targetImage != null)
            {
                Color color = targetImage.color;
                color.a = currentAlpha;
                targetImage.color = color;
            }
            else if (targetCanvasGroup != null)
            {
                targetCanvasGroup.alpha = currentAlpha;
            }
        }

        private void OnEnable()
        {
            // 每次启用时重置计时器，确保闪烁相位一致
            timer = 0f;
        }

        #endregion
    }
}
