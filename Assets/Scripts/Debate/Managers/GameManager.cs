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
        /// 玩家收集到的证据 ID 列表。
        /// </summary>
        // public List<string> collectedEvidence = new List<string>(); // 使用 BackpackManager 接管

        #endregion

        #region Unity 生命周期

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            // 如果处于菜单或暂停状态，不执行 Update
            /*
            if (!IsDebateLogicAllowed())
            {
                return;
            }
            */
            // Timer update logic removed
        }

        #endregion

        #region 状态管理方法

        /*
        /// <summary>
        /// 设置新的游戏状态。
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            Debug.Log($"[GameManager] State changed: {currentState} -> {newState}");
            currentState = newState;
            
            OnStateChanged(newState);
        }

        private void OnStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.UI_Menu:
                    Time.timeScale = 0f; // 暂停游戏时间
                    isGamePaused = true;
                    break;
                case GameState.Normal:
                case GameState.Dialogue:
                case GameState.Debate:
                case GameState.Cutscene:
                    Time.timeScale = 1f; // 恢复游戏时间
                    isGamePaused = false;
                    break;
            }
        }

        /// <summary>
        /// 检查是否允许玩家输入（移动、点击互动物品）。
        /// </summary>
        public bool IsInputAllowed()
        {
            return currentState == GameState.Normal;
        }

        /// <summary>
        /// 检查是否允许辩论系统的逻辑更新。
        /// </summary>
        public bool IsDebateLogicAllowed()
        {
            // 如果暂停了，或者打开了菜单，都不允许
            if (isGamePaused) return false;
            if (currentState == GameState.UI_Menu) return false;
            
            return true;
        }
        */

        #endregion

        #region 公共方法
        
        // ReduceTime method removed

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
