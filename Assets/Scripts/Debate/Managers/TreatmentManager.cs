using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DebateSystem;

namespace DebateSystem
{
    /// <summary>
    /// 剧本执行/导演管理器。
    /// 负责控制剧本的流程（上一句、下一句），并根据行类型（对话/辩论）分发控制权。
    /// </summary>
    public class TreatmentManager : Singleton<TreatmentManager>
    {
        #region Variables

        /// <summary>
        /// 当前正在执行的剧本数据。
        /// </summary>
        public DebateData currentScript;

        /// <summary>
        /// 当前行索引。
        /// </summary>
        public int currentIndex = 0;

        /// <summary>
        /// 是否正在等待用户点击（例如普通对话模式下）。
        /// </summary>
        public bool isWaitingForInput = false;

        // AutoPlay variables removed
        
        // 法庭卷轴动画是否正在进行
        private bool isCourtStageActive = false;
        
        [Header("Debug Settings")]
        /// <summary>
        /// 默认启动时加载的剧本文件名。
        /// </summary>
        public string defaultScriptFilename = "Act01_Chapter01_Trial.json";

        /// <summary>
        /// 获取当前行文本的辅助属性。
        /// </summary>
        public string CurrentLineText
        {
            get
            {
                if (currentScript != null && currentIndex < currentScript.lines.Count)
                {
                    return currentScript.lines[currentIndex].text;
                }
                return "";
            }
        }

        /// <summary>
        /// 根据章节 ID 自动拼接语音文件的完整路径。
        /// </summary>
        private string GetVoicePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "";
            if (fileName.Contains("/")) return fileName;

            string chapterId = currentScript.chapterId;
            string parentFolder = chapterId;

            int lastUnderscore = chapterId.LastIndexOf('_');
            if (lastUnderscore > 0)
            {
                string suffix = chapterId.Substring(lastUnderscore + 1);
                if (suffix.StartsWith("Trial") || suffix.StartsWith("Adv"))
                {
                    parentFolder = chapterId.Substring(0, lastUnderscore);
                }
            }

            return $"Audio/Voice/{parentFolder}/{chapterId}/{fileName}";
        }

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();
            // 防止 Inspector 中残留的序列化数据干扰
            currentScript = null;
        }

        private void Start()
        {
            Debug.Log("[Treatment] Start called, invoking DelayedStart");
            StartCoroutine(DelayedStart());
        }

        private void Update()
        {
            if (isWaitingForInput)
            {
                // AutoPlay logic removed

                // 鼠标点击逻辑
                if (Input.GetMouseButtonDown(0))
                {
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        if (DebateUIManager.Instance && DebateUIManager.Instance.IsTyping)
                        {
                            DebateUIManager.Instance.CompleteTyping(CurrentLineText);
                        }
                        else
                        {
                            isWaitingForInput = false;
                            NextLine();
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 加载并开始执行剧本。
        /// </summary>
        public void LoadScript(string filename)
        {
            if (TextManager.Instance == null) return;
            
            currentScript = TextManager.Instance.LoadScript(filename);
            if (currentScript != null)
            {
                if (AudioManager.Instance && AudioManager.Instance.BGMList != null && AudioManager.Instance.BGMList.Length > 0)
                {
                    AudioManager.Instance.PlayBGM(0);
                }

                currentIndex = 0;
                ExecuteCurrentLine();
            }
        }

        /// <summary>
        /// 执行当前索引所在的行。
        /// </summary>
        public void ExecuteCurrentLine()
        {
            if (currentScript == null || currentIndex >= currentScript.lines.Count)
            {
                Debug.Log("剧本执行结束 (Script finished).");
                return;
            }

            ScriptLine line = currentScript.lines[currentIndex];
            Debug.Log($"[Treatment] 执行行 {currentIndex}, 类型: {line.type}");

            switch (line.type)
            {
                case "dialogue":
                    HandleDialogue(line);
                    break;
                case "debate":
                    HandleDebate(line);
                    break;
                case "command":
                    HandleCommand(line);
                    break;
                default:
                    Debug.LogWarning($"未知行类型: {line.type}");
                    NextLine();
                    break;
            }
        }

        /// <summary>
        /// 前进到下一行。
        /// </summary>
        public void NextLine()
        {
            currentIndex++;
            ExecuteCurrentLine();
        }
        
        /// <summary>
        /// 跳转到指定 ID 的行。
        /// </summary>
        public void JumpToLine(string lineId)
        {
            int index = currentScript.lines.FindIndex(l => l.id == lineId);
            if (index != -1)
            {
                currentIndex = index;
                ExecuteCurrentLine();
            }
            else
            {
                Debug.LogError($"跳转目标未找到: {lineId}");
            }
        }

        // ToggleAutoPlay removed

        #endregion

        #region Internal Handlers

        /// <summary>
        /// 处理普通对话行。
        /// </summary>
        void HandleDialogue(ScriptLine line)
        {
            if (DebateUIManager.Instance)
            {
                Debug.Log("[Treatment] Calling ShowDialoguePanel(true)");
                DebateUIManager.Instance.ShowDialoguePanel(true);
                DebateUIManager.Instance.ShowDebatePanel(false);
                DebateUIManager.Instance.UpdateDialogue(line.speaker, line.text);
                
                if (!string.IsNullOrEmpty(line.portrait))
                {
                    if (!isCourtStageActive)
                    {
                        // 传递 distance 参数
                        DebateUIManager.Instance.SetPortrait(line.position, line.portrait, false, line.distance);
                    }
                    else
                    {
                        // 法庭动画模式下，更新卷轴中的角色立绘
                        if (DebateUIManager.Instance.courtStageController)
                        {
                            DebateUIManager.Instance.courtStageController.UpdateCharacterPortrait(line.portrait);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[Treatment] DebateUIManager.Instance is null!");
            }
            
            if (!string.IsNullOrEmpty(line.voice) && AudioManager.Instance)
            {
                string voicePath = GetVoicePath(line.voice);
                AudioManager.Instance.PlayVoice(voicePath);
            }
            
            isWaitingForInput = true;
        }

        /// <summary>
        /// 处理辩论环节。
        /// </summary>
        void HandleDebate(ScriptLine line)
        {
            CallDebateManager(line);
        }

        /// <summary>
        /// 实际调用 DebateManager 开始辩论逻辑。
        /// </summary>
        void CallDebateManager(ScriptLine line)
        {
            if (DebateUIManager.Instance)
            {
                DebateUIManager.Instance.ShowDialoguePanel(false);
                DebateUIManager.Instance.ShowDebatePanel(true);
                
                // 设置辩论立绘 (辩论模式下通常只有一个发言人)
                if (!string.IsNullOrEmpty(line.portrait))
                {
                    // 辩论模式暂时不需要 distance 参数，或者如果需要可以传入 line.distance
                    // 暂时保持原样，因为辩论模式UI结构不同，可能不需要缩放逻辑
                    DebateUIManager.Instance.SetPortrait(line.position, line.portrait, true);
                }
            }
            
            if (!string.IsNullOrEmpty(line.voice) && AudioManager.Instance)
            {
                string voicePath = GetVoicePath(line.voice);
                AudioManager.Instance.PlayVoice(voicePath);
            }
            
            if (DebateManager.Instance)
            {
                DebateManager.Instance.StartDebate(line.debateConfig, OnDebateFinished);
            }
            else
            {
                Debug.LogError("DebateManager 实例丢失！");
            }
        }

        /// <summary>
        /// 处理指令行。
        /// </summary>
        void HandleCommand(ScriptLine line)
        {
            if (string.IsNullOrEmpty(line.command))
            {
                NextLine();
                return;
            }

            switch (line.command)
            {
                case "SwitchCharacter":
                case "AddCharacter":
                    // 格式: parameters[0] = position, parameters[1] = path, parameters[2] (可选) = distance
                    if (line.parameters != null && line.parameters.Length >= 2)
                    {
                        string distance = "far";
                        if (line.parameters.Length >= 3)
                        {
                            distance = line.parameters[2];
                        }
                        
                        bool isDebate = DebateUIManager.Instance && DebateUIManager.Instance.debatePanel.activeSelf;
                        DebateUIManager.Instance.SetPortrait(line.parameters[0], line.parameters[1], isDebate, distance);
                    }
                    break;

                case "RemoveCharacter":
                    if (line.parameters != null && line.parameters.Length >= 1)
                    {
                        bool isDebate = DebateUIManager.Instance && DebateUIManager.Instance.debatePanel.activeSelf;
                        DebateUIManager.Instance.SetPortrait(line.parameters[0], "null", isDebate);
                    }
                    break;
                    
                case "TurnBg":
                    if (line.parameters != null && line.parameters.Length >= 1)
                    {
                        DebateUIManager.Instance.SetBackground(line.parameters[0]);
                    }
                    break;

                case "PlayBGM":
                    if (line.parameters != null && line.parameters.Length >= 1)
                    {
                        if (int.TryParse(line.parameters[0], out int bgmIndex))
                        {
                            if (AudioManager.Instance)
                            {
                                AudioManager.Instance.PlayBGM(bgmIndex);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"PlayBGM 参数错误: {line.parameters[0]} 不是有效的整数索引");
                        }
                    }
                    break;

                case "SwitchBGM":
                    if (line.parameters != null && line.parameters.Length >= 1)
                    {
                        if (int.TryParse(line.parameters[0], out int bgmIndex))
                        {
                            if (AudioManager.Instance)
                            {
                                AudioManager.Instance.SwitchBGM(bgmIndex);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"SwitchBGM 参数错误: {line.parameters[0]} 不是有效的整数索引");
                        }
                    }
                    break;

                case "StopBGM":
                    if (AudioManager.Instance)
                    {
                        AudioManager.Instance.StopBGM();
                    }
                    break;
                    
                case "ShowChoice":
                    if (line.parameters != null && line.parameters.Length > 0)
                    {
                        List<DebateOption> options = new List<DebateOption>();
                        List<string> targetIds = new List<string>();

                        for (int i = 0; i < line.parameters.Length; i++)
                        {
                            string param = line.parameters[i];
                            string text = param;
                            string targetId = "";

                            if (param.Contains("|"))
                            {
                                string[] parts = param.Split('|');
                                text = parts[0];
                                if (parts.Length > 1) targetId = parts[1];
                            }

                            options.Add(new DebateOption { 
                                id = $"opt_{i}", 
                                text = text 
                            });
                            targetIds.Add(targetId);
                        }

                        bool isDebate = DebateUIManager.Instance.debatePanel.activeSelf;
                        
                        DebateUIManager.Instance.ShowOptions((index) => {
                            string selectedTarget = targetIds[index];
                            Debug.Log($"选择了选项: {options[index].text}, 跳转至: {selectedTarget}");
                            
                            JumpToTarget(selectedTarget);
                        }, options, isDebate);
                        
                        isWaitingForInput = false; 
                        return;
                    }
                    else
                    {
                        // NextLine(); // Removed redundant call since we break to bottom
                    }
                    break;

                case "EndChapter":
                    Debug.Log("章节结束！");
                    return; 

                case "LoadNextScript":
                    if (line.parameters != null && line.parameters.Length > 0)
                    {
                        LoadScript(line.parameters[0]);
                    }
                    return;

                case "Jump":
                    if (line.parameters != null && line.parameters.Length > 0)
                    {
                        JumpToLine(line.parameters[0]);
                    }
                    return;

                case "InitCourtStage":
                    // 自动预读取立绘逻辑
                    if (DebateUIManager.Instance && DebateUIManager.Instance.courtStageController)
                    {
                        // 统一调用 ShowCourtStage 管理状态 (背景、面板、父对象)
                        DebateUIManager.Instance.ShowCourtStage(true);
                        
                        // 隐藏常规对话界面的所有立绘
                        DebateUIManager.Instance.SwitchPortraitVisibility("all", false, false);
                        
                        // 准备传递给 Controller 的数据列表
                        List<CourtStageController.CharacterRuntimeData> runtimeDataList = new List<CourtStageController.CharacterRuntimeData>();

                        // 优先使用结构化的 courtStageDef
                        if (line.courtStageDef != null && line.courtStageDef.Count > 0)
                        {
                            // 1. 提取名字用于查找立绘
                            List<string> names = new List<string>();
                            foreach (var def in line.courtStageDef)
                            {
                                names.Add(def.name);
                            }

                            // 2. 解析立绘路径
                            List<string> resolvedPaths = ResolvePortraitsForCharacters(names);

                            // 3. 组装数据
                            for (int i = 0; i < line.courtStageDef.Count; i++)
                            {
                                var def = line.courtStageDef[i];
                                string path = (i < resolvedPaths.Count) ? resolvedPaths[i] : "";
                                
                                runtimeDataList.Add(new CourtStageController.CharacterRuntimeData
                                {
                                    portraitPath = path,
                                    useCustomPos = def.useCustomPos,
                                    customPos = new Vector2(def.x, def.y)
                                });
                            }
                        }
                        else 
                        {
                            // 兼容旧的 parameters 方式
                            List<string> orderedCharNames = new List<string>();
                            if (line.parameters != null && line.parameters.Length > 0)
                            {
                                orderedCharNames.AddRange(line.parameters);
                            }
                            
                            List<string> resolvedPortraits = ResolvePortraitsForCharacters(orderedCharNames);
                            
                            foreach (var path in resolvedPortraits)
                            {
                                runtimeDataList.Add(new CourtStageController.CharacterRuntimeData
                                {
                                    portraitPath = path,
                                    useCustomPos = false
                                });
                            }
                        }
                        
                        // 3. 初始化全景动画
                        if (runtimeDataList.Count > 0)
                        {
                            isCourtStageActive = true;
                            DebateUIManager.Instance.courtStageController.Init(runtimeDataList);
                        }
                        else
                        {
                            Debug.LogWarning("[Treatment] InitCourtStage: No character data prepared!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Treatment] InitCourtStage failed: Missing controller.");
                    }
                    break;
                
                case "StopCourtStage":
                    if (DebateUIManager.Instance && DebateUIManager.Instance.courtStageController)
                    {
                        isCourtStageActive = false;
                        DebateUIManager.Instance.courtStageController.StopAndHide();
                        
                        // 统一恢复状态
                        DebateUIManager.Instance.ShowCourtStage(false);
                    }
                    break;

                case "AddEvidence":
                    // 格式: [0]ID, [1]Title, [2]Desc, [3]IconPath(Optional)
                    if (line.parameters != null && line.parameters.Length >= 3)
                    {
                        string eId = line.parameters[0];
                        string eTitle = line.parameters[1];
                        string eDesc = line.parameters[2];
                        string eIcon = line.parameters.Length > 3 ? line.parameters[3] : "";
                        
                        if (GameManager.Instance)
                        {
                            GameManager.Instance.AddEvidence(eId, eTitle, eDesc, eIcon);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Treatment] AddEvidence 参数不足，至少需要 ID, Title, Description");
                    }
                    break;

                case "CheckEvidence":
                    // 格式: [0]EvidenceID, [1]JumpIfTrue, [2]JumpIfFalse(Optional)
                    if (line.parameters != null && line.parameters.Length >= 2)
                    {
                        string checkId = line.parameters[0];
                        string trueTarget = line.parameters[1];
                        string falseTarget = line.parameters.Length > 2 ? line.parameters[2] : "";
                        
                        bool has = false;
                        if (GameManager.Instance)
                        {
                            has = GameManager.Instance.HasEvidence(checkId);
                        }
                        
                        if (has)
                        {
                            Debug.Log($"[Treatment] 拥有证物 {checkId}，跳转至 {trueTarget}");
                            JumpToTarget(trueTarget);
                            return; // JumpToTarget 会决定是否 return，但这里直接 return 更安全
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(falseTarget))
                            {
                                Debug.Log($"[Treatment] 未拥有证物 {checkId}，跳转至 {falseTarget}");
                                JumpToTarget(falseTarget);
                                return;
                            }
                            // 如果没有 falseTarget，继续执行下一行
                        }
                    }
                    break;

                default:
                    Debug.LogWarning($"未知指令: {line.command}");
                    break;
            }
            
            NextLine();
        }

        /// <summary>
        /// 辩论结束后的回调。
        /// </summary>
        void OnDebateFinished(bool success, string nextLineId)
        {
            if (success)
            {
                if (!string.IsNullOrEmpty(nextLineId))
                    JumpToLine(nextLineId);
                else
                    NextLine();
            }
            else
            {
                Debug.Log("辩论失败，重新开始本轮...");
                ExecuteCurrentLine();
            }
        }

        #endregion

        #region Helper Methods

        IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();
            Debug.Log("[Treatment] DelayedStart executing...");
            // 如果还没有加载任何剧本（例如没有被外部管理器调用 LoadScript），则加载默认剧本
            if (currentScript == null && !string.IsNullOrEmpty(defaultScriptFilename))
            {
                Debug.Log($"[Treatment] Loading default script: {defaultScriptFilename}");
                LoadScript(defaultScriptFilename);
            }
            else
            {
                 Debug.Log($"[Treatment] Skipped loading default script. currentScript exists: {currentScript != null}, chapterId: {currentScript?.chapterId}");
            }
        }



        private void JumpToTarget(string targetId)
        {
            if (!string.IsNullOrEmpty(targetId))
            {
                JumpToLine(targetId);
            }
            else
            {
                NextLine();
            }
        }

        /// <summary>
        /// 根据给定的角色名列表，从后续对话中解析对应的立绘路径。
        /// </summary>
        private List<string> ResolvePortraitsForCharacters(List<string> orderedNames)
        {
            Dictionary<string, string> nameToPathMap = new Dictionary<string, string>();
            
            for (int i = currentIndex + 1; i < currentScript.lines.Count; i++)
            {
                ScriptLine line = currentScript.lines[i];
                if (line.type == "command" && line.command == "StopCourtStage") break;

                if (line.type == "dialogue" && !string.IsNullOrEmpty(line.portrait) && line.portrait != "null")
                {
                    string[] parts = line.portrait.Split('/');
                    if (parts.Length > 0)
                    {
                        string charKey = parts[0]; 
                        if (!nameToPathMap.ContainsKey(charKey))
                        {
                            nameToPathMap[charKey] = line.portrait;
                        }
                    }
                }
            }

            List<string> finalPaths = new List<string>();
            foreach (var name in orderedNames)
            {
                if (nameToPathMap.ContainsKey(name))
                {
                    finalPaths.Add(nameToPathMap[name]);
                }
                else
                {
                    Debug.LogWarning($"[Treatment] 未在对话中找到角色 {name} 的立绘，尝试使用默认路径猜测。");
                    finalPaths.Add($"{name}/{name}_Default"); 
                }
            }
            
            return finalPaths;
        }

        #endregion
    }
}
