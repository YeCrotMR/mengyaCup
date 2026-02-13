using UnityEngine;
using System.Collections.Generic;

namespace DebateSystem
{
    /// <summary>
    /// 全局游戏管理器。
    /// 负责管理游戏核心状态（如倒计时、证据背包）。
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        #region 变量定义

        /*
        // 游戏状态（Game State）
        public enum GameState
        {
            Normal,         // 正常探索/游戏
            Dialogue,       // 对话中 (DialogueSystem)
            Debate,         // 辩论中 (DebateSystem)
            UI_Menu,        // 菜单/背包打开中 (UIManager)
            Cutscene        // 动画演出中
        }

        [Header("Global State Control")]
        [SerializeField] private GameState currentState = GameState.Normal;
        public GameState CurrentState => currentState;

        /// <summary>
        /// 游戏是否暂停。
        /// </summary>
        public bool isGamePaused = false;
        */
        
        /// <summary>
        /// 角色嫌疑度字典 (ID -> Value)
        /// </summary>
        public Dictionary<string, int> characterSuspicion = new Dictionary<string, int>();

        #endregion

        #region Unity 生命周期

        protected override void Awake()
        {
            base.Awake();
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region 公共方法
        
        // ReduceTime method removed

        /// <summary>
        /// 增加角色嫌疑度
        /// </summary>
        public void AddSuspicion(string charId, int amount)
        {
            if (!characterSuspicion.ContainsKey(charId))
            {
                characterSuspicion[charId] = 0;
            }
            characterSuspicion[charId] += amount;
            Debug.Log($"[GameManager] 角色 {charId} 嫌疑度增加 {amount}, 当前: {characterSuspicion[charId]}");
            
            // 如果需要更新UI或背包显示，可以在这里发送事件
        }

        /// <summary>
        /// 获取角色嫌疑度
        /// </summary>
        public int GetSuspicion(string charId)
        {
            if (characterSuspicion.ContainsKey(charId))
                return characterSuspicion[charId];
            return 0;
        }

        /// <summary>
        /// 添加证据到背包。
        /// </summary>
        public void AddEvidence(string evidenceId, string title = "", string description = "", string iconPath = "")
        {
            // 优先使用 BackpackManager
            if (BackpackManager.Instance)
            {
                Sprite icon = null;
                if (!string.IsNullOrEmpty(iconPath))
                {
                    icon = Resources.Load<Sprite>($"Image/Evidence/{iconPath}");
                    if (icon == null) icon = Resources.Load<Sprite>(iconPath); // 尝试完整路径
                }

                // 如果没有提供标题和描述，尝试使用默认值或警告
                if (string.IsNullOrEmpty(title)) title = "未知证物";
                if (string.IsNullOrEmpty(description)) description = "（暂无描述）";

                BackpackManager.Instance.AddEvidence(evidenceId, title, description, icon);
                Debug.Log($"[GameManager] 已通过 BackpackManager 添加证物: {evidenceId}");
            }
            else
            {
                Debug.LogWarning("[GameManager] BackpackManager 实例未找到！无法添加证物。");
            }
        }

        /// <summary>
        /// 检查是否拥有某个证据。
        /// </summary>
        public bool HasEvidence(string evidenceId)
        {
            if (BackpackManager.Instance)
            {
                return BackpackManager.Instance.HasItem(evidenceId);
            }
            return false;
        }

        #endregion

        #region 私有方法

        // GameOver method removed

        #endregion
    }
}
