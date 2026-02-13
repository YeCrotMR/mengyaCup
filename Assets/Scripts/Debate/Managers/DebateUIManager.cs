using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DebateSystem;

namespace DebateSystem
{
    /// <summary>
    /// UI 管理器。
    /// 负责所有 UI 面板的显示、隐藏以及内容更新（立绘、文本）。
    /// </summary>
    public class DebateUIManager : Singleton<DebateUIManager>
    {
        #region 变量定义

        [Header("Main Panels (主面板)")]
        public GameObject dialoguePanel;   // 对应 DialogPanel
        public GameObject debatePanel;   // 对应 DebatePanel
        public GameObject menuPanel;     // 对应 MenuPanel
        
        [Header("Options UI (选项面板)")]
        // 辩论模式选项
        public GameObject debateOptionsPanel;  // 对应 DebatePanel 下的 TrialChoice

        // 对话模式选项
        public GameObject dialogueOptionsPanel; // 对应 DialogPanel 下的 DialogChoice

        [Header("Evidence Panel (证据面板)")]
        public GameObject evidencePanel; 
        
        [Header("Background (背景)")]
        public Image sceneBG;            // 对应 SceneBG

        [Header("Dialogue UI (普通对话组件)")]
        public TextMeshProUGUI speakerName;         // 对应 SpeakerName -> Text (TMP)
        public TextMeshProUGUI dialogText;          // 对应 DialogText -> Text (TMP)
        public Image leftShow;           // 对应 LeftMask -> LeftShow
        public Image centerShow;         // 对应 CenterMask -> CenterShow
        public Image rightShow;          // 对应 RightMask -> RightShow

        [Header("Debate UI (辩论组件)")]
        public GameObject debateSpeakPos; // 对应 SpeakPos
        public GameObject debateText; 
        public Image debateLeftShow;
        public Image debateRightShow;
        
        public TextMeshProUGUI debateTextLeft;     // 对应 DebateText (左侧显示)
        public TextMeshProUGUI debateTextRight;    // 对应 DebateText (右侧显示)      

        [Header("Court Stage (法庭全景卷轴)")]
        public CourtStageController courtStageController; 

        [Header("Text Settings (文本设置)")]
        public float typeSpeed = 0.05f; // 普通对话速度
        public float debateTypeSpeed = 0.1f; // 辩论打字速度（较慢）
        private Coroutine typingCoroutine;

        /// <summary>
        /// 是否正在进行打字机效果。
        /// </summary>
        public bool IsTyping => typingCoroutine != null;

        // 缓存引用字典
        private Dictionary<string, Image> dialoguePortraits;
        private Dictionary<string, Image> debatePortraits;
        
        // 缓存初始位置和缩放，用于恢复 far 状态
        private Dictionary<Image, Vector2> initialPositions = new Dictionary<Image, Vector2>();
        private Dictionary<Image, Vector3> initialScales = new Dictionary<Image, Vector3>();

        #endregion

        #region Unity 生命周期

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("[UI] DebateUIManager Awake called.");
            CachePortraitReferences();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 更新辩论面板的UI状态。
        /// </summary>
        /// <param name="text">对话文本</param>
        /// <param name="position">立绘位置 (left/right)</param>
        /// <param name="portraitPath">立绘路径</param>
        public void UpdateDebateUI(string text, string position, string portraitPath)
        {
            if (!debatePanel || !debatePanel.activeSelf) ShowDebatePanel(true);

            // 1. 重置所有立绘和文本框
            if (debateLeftShow) debateLeftShow.color = new Color(1, 1, 1, 0);
            if (debateRightShow) debateRightShow.color = new Color(1, 1, 1, 0);

            // 2. 根据位置设置立绘和文本
            // 规则: 左立绘 -> 右文本; 右立绘 -> 左文本
            string posKey = position.ToLower();
            TextMeshProUGUI targetText = null;
            
            if (posKey == "left")
            {
                SetPortrait("left", portraitPath, true);
                if (debateTextRight) 
                {
                    targetText = debateTextRight;
                    debateTextRight.gameObject.SetActive(true);
                }
                if (debateTextLeft) debateTextLeft.gameObject.SetActive(false);
            }
            else if (posKey == "right")
            {
                SetPortrait("right", portraitPath, true);
                if (debateTextLeft) 
                {
                    targetText = debateTextLeft;
                    debateTextLeft.gameObject.SetActive(true);
                }
                if (debateTextRight) debateTextRight.gameObject.SetActive(false);
            }
            else // center or others
            {
                SetPortrait(posKey, portraitPath, true);
                // 默认显示在左边或根据需求
                if (debateTextLeft) 
                {
                    targetText = debateTextLeft;
                    debateTextLeft.gameObject.SetActive(true);
                }
                if (debateTextRight) debateTextRight.gameObject.SetActive(false);
            }

            // 3. 执行打字机效果
            if (targetText != null)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                targetText.text = ""; // 先清空
                // 辩论模式使用较慢的速度
                typingCoroutine = StartCoroutine(TypewriterEffect(targetText, text, debateTypeSpeed));
            }
        }

        /// <summary>
        /// 显示/隐藏普通对话面板。
        /// </summary>
        public void ShowDialoguePanel(bool show)
        {
            if (dialoguePanel) 
            {
                dialoguePanel.SetActive(show);
                Debug.Log($"[UI] Setting DialoguePanel active: {show}");
            }
            else
            {
                Debug.LogError("[UI] dialoguePanel reference is missing!");
            }
        }

        /// <summary>
        /// 显示/隐藏辩论面板。
        /// </summary>
        public void ShowDebatePanel(bool show)
        {
            if (debatePanel) 
            {
                debatePanel.SetActive(show);
                if (show)
                {
                    // 初始隐藏左右文本框，等待 UpdateDebateUI 激活其中一个
                    if (debateTextLeft) debateTextLeft.gameObject.SetActive(false);
                    if (debateTextRight) debateTextRight.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 显示/隐藏菜单面板。
        /// </summary>
        public void ShowMenuPanel(bool show)
        {
            if (menuPanel) menuPanel.SetActive(show);
        }

        /// <summary>
        /// 切换菜单面板的显示状态。
        /// </summary>
        public void ToggleMenuPanel()
        {
            if (menuPanel)
            {
                menuPanel.SetActive(!menuPanel.activeSelf);
            }
        }

        /// <summary>
        /// 控制法庭卷轴动画阶段的 UI 状态。
        /// </summary>
        /// <param name="show">是否显示法庭卷轴</param>
        public void ShowCourtStage(bool show)
        {
            if (courtStageController)
            {
                // 控制 CourtStage 显隐 (如果 Controller 没有完全接管)
                courtStageController.gameObject.SetActive(show);
            }

            // 控制 SceneBG (互斥)
            if (sceneBG)
            {
                sceneBG.gameObject.SetActive(!show);
            }

            // 控制面板状态
            if (show)
            {
                // 卷轴模式：显示对话框，隐藏辩论面板
                ShowDialoguePanel(true);
                ShowDebatePanel(false);
            }
            else
            {
                // 退出卷轴模式：通常回到对话模式，但也可能直接进辩论
                // 默认回到对话模式
                ShowDialoguePanel(true);
                ShowDebatePanel(false);
            }
        }

        /// <summary>
        /// 更新对话框内容（支持打字机效果）。
        /// </summary>
        public void UpdateDialogue(string speaker, string text)
        {
            if (speakerName) speakerName.text = speaker;
            
            if (dialogText)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                
                typingCoroutine = StartCoroutine(TypewriterEffect(dialogText, text, typeSpeed));
            }
        }

        /// <summary>
        /// 立即完成打字效果（显示整句话）。
        /// </summary>
        public void CompleteTyping(string fullText)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            // 尝试完成当前的打字目标（无论是对话还是辩论）
            if (dialogText && dialogText.gameObject.activeInHierarchy)
            {
                dialogText.maxVisibleCharacters = 99999;
                dialogText.text = fullText;
            }
            else if (debateTextLeft && debateTextLeft.gameObject.activeInHierarchy)
            {
                debateTextLeft.maxVisibleCharacters = 99999;
                debateTextLeft.text = fullText;
            }
            else if (debateTextRight && debateTextRight.gameObject.activeInHierarchy)
            {
                debateTextRight.maxVisibleCharacters = 99999;
                debateTextRight.text = fullText;
            }
        }

        /// <summary>
        /// 设置背景图片。
        /// </summary>
        /// <param name="path">相对于 Resources/Image/Background/ 的路径</param>
        public void SetBackground(string path)
        {
            if (sceneBG != null)
            {
                if (string.IsNullOrEmpty(path))
                {
                    // Do nothing
                }
                else
                {
                    string fullPath = $"Image/Background/{path}";
                    Sprite bgSprite = Resources.Load<Sprite>(fullPath);
                    if (bgSprite != null)
                    {
                        sceneBG.sprite = bgSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"[UI] Background not found: {fullPath}");
                    }
                }
            }
        }

        /// <summary>
        /// 设置指定位置的立绘。
        /// </summary>
        /// <param name="position">位置 ("left", "right", "center")</param>
        /// <param name="path">资源路径 (相对于 Resources/Image/Character/)</param>
        /// <param name="isDebateMode">是否是辩论模式（决定更新哪个面板的图片）</param>
        /// <param name="distance">距离 ("far", "near")。默认 "far"。</param>
        public void SetPortrait(string position, string path, bool isDebateMode = false, string distance = "far")
        {
            Dictionary<string, Image> targetDict = isDebateMode ? debatePortraits : dialoguePortraits;
            string key = position.ToLower();

            if (targetDict != null && targetDict.ContainsKey(key))
            {
                Image target = targetDict[key];
                if (target)
                {
                    if (string.IsNullOrEmpty(path) || path == "hide" || path == "null")
                    {
                        target.sprite = null; 
                        target.color = new Color(1, 1, 1, 0); 
                    }
                    else
                    {
                        target.gameObject.SetActive(true);
                        string fullPath = $"Image/Character/{path}";
                        Sprite sprite = Resources.Load<Sprite>(fullPath);
                        
                        if (sprite != null)
                        {
                            target.sprite = sprite;
                            target.SetNativeSize(); 
                            
                            RectTransform rect = target.GetComponent<RectTransform>();
                            if (rect)
                            {
                                // 基础尺寸调整 (保持原大，无需除以2)
                                Vector2 baseSize = rect.sizeDelta;
                                // rect.sizeDelta = baseSize / 2f; // 已移除除以2的逻辑
                                
                                // 根据 distance 处理位置和缩放
                                // 默认 far: 保持原样 (假设 Unity 编辑器里默认摆放的是 far 的位置)
                                // near: Y轴为 -1309，缩放为 2.4
                                if (distance == "near")
                                {
                                    // 设置 Y 坐标
                                    Vector2 anchoredPos = rect.anchoredPosition;
                                    anchoredPos.y = -1150f;
                                    rect.anchoredPosition = anchoredPos;
                                    
                                    // 设置缩放
                                    rect.localScale = new Vector3(2.3f, 2.3f, 2.3f);
                                }
                                else
                                {
                                    // far (恢复默认)
                                    if (initialPositions.ContainsKey(target) && initialScales.ContainsKey(target))
                                    {
                                        rect.anchoredPosition = initialPositions[target];
                                        rect.localScale = initialScales[target];
                                    }
                                    else
                                    {
                                        // 兜底：如果没有记录到初始值，重置为常见默认值
                                        // 注意：这里可能不准确，所以最好确保 Awake 时记录了正确的值
                                        rect.localScale = Vector3.one;
                                    }
                                }
                            }

                            target.preserveAspect = true; 
                            target.color = new Color(1, 1, 1, 1); 
                        }
                        else
                        {
                            Debug.LogWarning($"[UI] 立绘未找到: Resources/{fullPath}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[UI] 未知立绘位置: {position}");
            }
        }

        /// <summary>
        /// 开关立绘显示。
        /// </summary>
        /// <param name="position">位置 ("left", "right", "center", "all")</param>
        /// <param name="show">是否显示</param>
        /// <param name="isDebateMode">是否是辩论模式</param>
        public void SwitchPortraitVisibility(string position, bool show, bool isDebateMode = false)
        {
            Dictionary<string, Image> targetDict = isDebateMode ? debatePortraits : dialoguePortraits;
            string key = position.ToLower();
            float alpha = show ? 1f : 0f;

            if (key == "all")
            {
                foreach (var img in targetDict.Values)
                {
                    if (img) img.color = new Color(1, 1, 1, alpha);
                }
            }
            else if (targetDict.ContainsKey(key))
            {
                if (targetDict[key]) targetDict[key].color = new Color(1, 1, 1, alpha);
            }
        }
        
        /// <summary>
        /// 显示选项菜单（复用场景中预置的按钮）。
        /// </summary>
        /// <param name="onSelected">选择后的回调函数 (返回索引)</param>
        /// <param name="options">选项数据列表</param>
        /// <param name="isDebateMode">是否是辩论模式</param>
        public void ShowOptions(System.Action<int> onSelected, List<DebateOption> options, bool isDebateMode = true)
        {
            GameObject targetPanel = isDebateMode ? debateOptionsPanel : dialogueOptionsPanel;
            if (targetPanel == null)
            {
                Debug.LogError("[UIManager] Options Panel is not assigned!");
                return;
            }

            Transform choiceGroup = targetPanel.transform.Find("ChoiceGroup");
            if (choiceGroup == null)
            {
                Debug.LogError($"[UIManager] ChoiceGroup not found under {targetPanel.name}");
                return;
            }

            targetPanel.SetActive(true);

            List<Button> optionButtons = new List<Button>();
            foreach (Transform child in choiceGroup)
            {
                // 如果是辩论模式，我们需要包含所有按钮（包括可能被命名为 Cancel 的按钮）
                // 如果是对话模式，保持原有逻辑
                if (!isDebateMode && child.name == "Cancel") continue;
                
                Button btn = child.GetComponent<Button>();
                if (btn) optionButtons.Add(btn);
            }

            // 辩论模式特殊布局逻辑
            if (isDebateMode && optionButtons.Count >= 4)
            {
                // 先隐藏所有按钮
                foreach (var btn in optionButtons) btn.gameObject.SetActive(false);

                // 最后一个按钮始终作为返回按钮 (Index 3)
                Button returnBtn = optionButtons[optionButtons.Count - 1];
                returnBtn.gameObject.SetActive(true);
                returnBtn.onClick.RemoveAllListeners();
                returnBtn.onClick.AddListener(() => {
                    targetPanel.SetActive(false);
                    onSelected(-1); // -1 表示取消/返回
                });

                // 根据选项数量填充按钮
                // 1个选项: 显示 Index 2 (倒数第二个)
                // 2个选项: 显示 Index 2, 1
                // 3个选项: 显示 Index 2, 1, 0
                
                int count = options.Count;
                
                // 映射规则：选项0 -> Index 2, 选项1 -> Index 1, 选项2 -> Index 0
                // 注意：这里假设 buttons 列表顺序是 Hierarchy 里的顺序 (0, 1, 2, 3)
                // Index 3 是返回按钮
                
                if (count >= 1)
                {
                    // 选项 0 放在倒数第二个位置 (Index 2)
                    SetupButton(optionButtons[optionButtons.Count - 2], options[0].text, 0, targetPanel, onSelected);
                }
                
                if (count >= 2)
                {
                    // 选项 1 放在倒数第三个位置 (Index 1)
                    SetupButton(optionButtons[optionButtons.Count - 3], options[1].text, 1, targetPanel, onSelected);
                }
                
                if (count >= 3)
                {
                    // 选项 2 放在倒数第四个位置 (Index 0)
                    SetupButton(optionButtons[optionButtons.Count - 4], options[2].text, 2, targetPanel, onSelected);
                }
            }
            else
            {
                // 原有逻辑 (对话模式或按钮数量不足时的回退)
                for (int i = 0; i < optionButtons.Count; i++)
                {
                    Button btn = optionButtons[i];
                    
                    if (i < options.Count)
                    {
                        SetupButton(btn, options[i].text, i, targetPanel, onSelected);
                    }
                    else
                    {
                        btn.gameObject.SetActive(false);
                    }
                }
            }

            if (options.Count > (isDebateMode ? 3 : optionButtons.Count))
            {
                Debug.LogWarning($"[UIManager] Not enough option buttons! Options: {options.Count}");
            }
        }

        private void SetupButton(Button btn, string text, int index, GameObject panel, System.Action<int> onSelected)
        {
            btn.gameObject.SetActive(true);
            
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText) btnText.text = text;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                panel.SetActive(false);
                onSelected(index);
            });
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 缓存立绘位置引用，使用字典管理，替代 switch-case。
        /// 同时记录初始位置和缩放。
        /// </summary>
        private void CachePortraitReferences()
        {
            dialoguePortraits = new Dictionary<string, Image>
            {
                { "left", leftShow },
                { "center", centerShow },
                { "right", rightShow }
            };

            debatePortraits = new Dictionary<string, Image>
            {
                { "left", debateLeftShow },
                { "right", debateRightShow }
            };

            // 记录初始状态 (假设 Awake 时是 far 状态)
            RecordInitialState(dialoguePortraits);
            RecordInitialState(debatePortraits);
        }

        private void RecordInitialState(Dictionary<string, Image> portraits)
        {
            foreach (var kvp in portraits)
            {
                Image img = kvp.Value;
                if (img)
                {
                    RectTransform rect = img.GetComponent<RectTransform>();
                    if (rect)
                    {
                        if (!initialPositions.ContainsKey(img))
                            initialPositions.Add(img, rect.anchoredPosition);
                        
                        if (!initialScales.ContainsKey(img))
                            initialScales.Add(img, rect.localScale);
                    }
                }
            }
        }

        /// <summary>
        /// 打字机效果协程。
        /// </summary>
        IEnumerator TypewriterEffect(TextMeshProUGUI target, string fullText, float speed)
        {
            target.text = fullText;
            target.maxVisibleCharacters = 0;
            
            // 强制刷新一次以获取正确的字符数量（忽略标签）
            target.ForceMeshUpdate();
            int validCharCount = target.textInfo.characterCount;

            for (int i = 0; i <= validCharCount; i++)
            {
                target.maxVisibleCharacters = i;
                yield return new WaitForSeconds(speed);
            }
            
            typingCoroutine = null;
        }

        #endregion
    }
}
