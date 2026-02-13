using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DebateSystem;
using TMPro;

namespace DebateSystem
{
    /// <summary>
    /// 辩论环节管理器。
    /// 负责自动播放语音、显示对话文本（通过 UIManager），并处理富文本点击交互。
    /// </summary>
    public class DebateManager : Singleton<DebateManager>
    {
        #region 变量定义

        private DebateRoundConfig currentConfig;
        private System.Action<bool, string> onFinishedCallback;
        private bool isDebating = false;

        // 当前正在显示的文本框引用，用于检测点击
        private TextMeshProUGUI currentActiveText = null;

        #endregion

        #region Unity 生命周期

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            if (isDebating && currentActiveText != null)
            {
                // 检测鼠标左键点击
                if (Input.GetMouseButtonDown(0))
                {
                    CheckLinkClick();
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 开始一个新的辩论回合。
        /// </summary>
        /// <param name="config">辩论配置数据</param>
        /// <param name="callback">结束时的回调 (success, nextLineId)</param>
        public void StartDebate(DebateRoundConfig config, System.Action<bool, string> callback)
        {
            currentConfig = config;
            onFinishedCallback = callback;
            isDebating = true;
            currentActiveText = null;
            
            StartCoroutine(DebateLoop());
        }



        #endregion

        #region 内部逻辑

        /// <summary>
        /// 辩论主循环：更新UI并等待语音播放结束。
        /// </summary>
        IEnumerator DebateLoop()
        {
            // 在新的逻辑下，一个 Debate 行通常只包含一个 sentence
            // 但如果包含多个，我们依次播放
            if (currentConfig.sentences != null && currentConfig.sentences.Count > 0)
            {
                for (int i = 0; i < currentConfig.sentences.Count; i++)
                {
                    if (!isDebating) yield break;

                    var data = currentConfig.sentences[i];
                    
                    // 1. 获取当前发言人的位置和立绘
                    // 由于 DebateManager 不持有这些信息，我们需要从 TreatmentManager 或上层传递
                    // 但目前的接口限制，我们假设 TreatmentManager 在调用 StartDebate 前已经设置好了 currentLine
                    // 实际上，DebateRoundConfig 里的 sentences 并不包含 speaker info，这些在 ScriptLine 里
                    // 变通方案：我们通过 TreatmentManager 获取当前行信息
                    
                    string position = "left"; // 默认
                    string portrait = "";
                    string voice = "";
                    
                    if (TreatmentManager.Instance && TreatmentManager.Instance.currentScript != null)
                    {
                        var line = TreatmentManager.Instance.currentScript.lines[TreatmentManager.Instance.currentIndex];
                        
                        // 注意：ScriptLine 中的 position/portrait/voice 是顶层配置
                        // 而 DebateRoundConfig.sentences 里的数据可能需要覆盖顶层配置
                        // 但目前 JSON 结构显示 sentences 里没有 speaker/portrait/voice
                        // 所以我们主要依赖 line (ScriptLine) 的数据
                        // 修正：如果 sentences 里有特定的说话人变更需求，JSON 需要支持
                        // 目前我们直接使用 ScriptLine 的数据
                        
                        position = line.position;
                        portrait = line.portrait;
                        voice = line.voice; 
                    }

                    // 2. 更新 UI
                    if (DebateUIManager.Instance)
                    {
                        // 自动为 link="weak" 添加颜色标签
                        string processedText = System.Text.RegularExpressions.Regex.Replace(data.text, 
                            "<link=\"weak\">(.*?)</link>", 
                            "<link=\"weak\"><color=#FF55AA>$1</color></link>");
                        
                        DebateUIManager.Instance.UpdateDebateUI(processedText, position, portrait);
                        
                        // 更新当前活动的文本框引用，用于点击检测
                        string posKey = position.ToLower();
                        if (posKey == "left")
                            currentActiveText = DebateUIManager.Instance.debateTextRight;
                        else if (posKey == "right")
                            currentActiveText = DebateUIManager.Instance.debateTextLeft;
                        else
                            currentActiveText = DebateUIManager.Instance.debateTextLeft;
                    }

                    // 3. 等待语音播放结束
                    // 给予一点缓冲时间，防止 IsVoicePlaying 还没变 true 就检测了
                    yield return new WaitForSeconds(0.1f);
                    
                    while (AudioManager.Instance && AudioManager.Instance.IsVoicePlaying())
                    {
                        if (!isDebating) yield break;
                        yield return null;
                    }
                    
                    // 语音结束后的额外停顿 (可选)
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                // 没有句子，仅作为占位符
                yield return new WaitForSeconds(1.0f);
            }

            // 所有句子播放完毕，且没有被打断（未触发弱点），自动结束
            if (isDebating)
            {
                EndDebate(true, null); // true 表示正常播放完毕，进入下一句
            }
        }

        /// <summary>
        /// 检测是否点击了 TMP 中的 Link
        /// </summary>
        void CheckLinkClick()
        {
            if (currentActiveText == null) return;

            // 获取摄像机
            Camera uiCamera = null;
            Canvas canvas = currentActiveText.canvas;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                uiCamera = null;
            else
                uiCamera = canvas.worldCamera;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(currentActiveText, Input.mousePosition, uiCamera);
            
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = currentActiveText.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();
                
                if (linkId == "weak")
                {
                    TriggerWeakPoint();
                }
            }
        }

        void TriggerWeakPoint()
        {
            Debug.Log("击中弱点！(Weak point hit!)");
            
            // 暂停协程
            StopAllCoroutines(); 
            // 注意：StopAllCoroutines 会停止 DebateLoop，这正是我们想要的
            // 但如果我们要恢复播放，就需要重新启动协程并跳转到当前位置，比较复杂
            // 简单起见，如果取消选择，我们重启本句辩论（重新调用 StartDebate 外部会比较麻烦）
            // 或者：我们只设置 timeScale = 0
            
            // 这里为了简单，我们暂停 TimeScale
            Time.timeScale = 0; 

            DebateUIManager.Instance.ShowOptions((index) => {
                Time.timeScale = 1; 
                CheckOption(index);
            }, currentConfig.options);
        }



        /// <summary>
        /// 验证玩家选择的选项是否正确。
        /// </summary>
        void CheckOption(int index)
        {
            // 如果是在选项面板点击了，说明已经停止了循环
            
            if (index >= 0 && index < currentConfig.options.Count)
            {
                var option = currentConfig.options[index];
                if (option.isCorrect)
                {
                    // 答对：跳转
                    EndDebate(true, option.nextLineId);
                }
                else
                {
                    // 答错：扣时
                    Debug.Log("选项错误！(Wrong option!)");
                    if (GameManager.Instance)
                    {
                        // GameManager.Instance.ReduceTime(option.penalty); // Removed penalty logic
                    }
                    
                    // 检查是否配置了跳转ID（即使是错误选项）
                    if (!string.IsNullOrEmpty(option.nextLineId))
                    {
                        // 如果有跳转ID，则结束辩论并跳转（通常用于显示错误剧情对话）
                        // 传入 true 表示流程上的“成功跳转”，而非逻辑上的“答对”
                        EndDebate(true, option.nextLineId);
                    }
                    else
                    {
                        // 恢复播放：重新启动 Loop
                        isDebating = true;
                        StartCoroutine(DebateLoop());
                    }
                }
            }
            else
            {
                // 如果是取消操作（例如点击了背景关闭选项面板），也应该恢复
                isDebating = true;
                StartCoroutine(DebateLoop());
            }
        }

        /// <summary>
        /// 结束辩论环节。
        /// </summary>
        void EndDebate(bool success, string nextId)
        {
            isDebating = false;
            StopAllCoroutines();
            onFinishedCallback?.Invoke(success, nextId);
        }



        #endregion
    }
}
