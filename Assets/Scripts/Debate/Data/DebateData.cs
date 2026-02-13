using System.Collections.Generic;
using UnityEngine;

namespace DebateSystem
{
    /// <summary>
    /// 辩论系统的数据根类，用于反序列化 JSON 剧本文件。
    /// </summary>
    [System.Serializable]
    public class DebateData
    {
        /// <summary>
        /// 章节 ID，用于标识当前剧本。
        /// </summary>
        public string chapterId;

        /// <summary>
        /// 剧本行列表，包含对话、辩论配置等。
        /// </summary>
        public List<ScriptLine> lines;
    }

    /// <summary>
    /// 剧本中的单行数据，可以是普通对话、辩论环节或指令。
    /// </summary>
    [System.Serializable]
    public class ScriptLine
    {
        /// <summary>
        /// 该行的唯一标识符，用于跳转。
        /// </summary>
        public string id;

        /// <summary>
        /// 行类型：
        /// "dialogue" - 普通对话
        /// "debate" - 进入辩论环节
        /// "command" - 执行特定指令
        /// </summary>
        public string type; 

        // ========== 对话字段 (Dialogue Fields) ==========

        /// <summary>
        /// 发言角色名称。
        /// </summary>
        public string speaker;

        /// <summary>
        /// 对话文本内容。
        /// </summary>
        public string text;

        /// <summary>
        /// 立绘路径（相对于 Resources/Image/Character/）。
        /// 例如："Maincharacter/主角1"
        /// </summary>
        public string portrait; 

        /// <summary>
        /// 语音文件路径（相对于 Resources/Voice/）。
        /// </summary>
        public string voice;

        /// <summary>
        /// 立绘显示位置："left", "center", "right"
        /// </summary>
        public string position; 

        /// <summary>
        /// 立绘远近状态："far", "near"
        /// far: 默认状态，正常显示 (远离屏幕)
        /// near: 靠近屏幕，Y轴为 -1309，缩放为 2.4
        /// </summary>
        public string distance; 

        // ========== 辩论字段 (Debate Fields) ==========

        /// <summary>
        /// 辩论环节的详细配置（仅当 type="debate" 时有效）。
        /// </summary>
        public DebateRoundConfig debateConfig;

        // ========== 指令字段 (Command Fields) ==========

        /// <summary>
        /// 指令名称（例如 "TurnBg"）。
        /// </summary>
        public string command;

        /// <summary>
        /// 指令参数列表。
        /// 常用指令参数说明：
        /// - AddEvidence: [0]ID, [1]标题, [2]描述, [3]图标路径(可选)
        /// - CheckEvidence: [0]ID, [1]拥有时跳转ID, [2]未拥有时跳转ID(可选)
        /// - SwitchCharacter: [0]位置, [1]路径, [2]距离(可选)
        /// - ShowChoice: [0]选项1|跳转ID, [1]选项2|跳转ID...
        /// </summary>
        public string[] parameters;

        // ========== 法庭全景动画配置 (Court Stage Config) ==========
        
        /// <summary>
        /// 法庭全景动画的角色配置列表（仅当 command="InitCourtStage" 时有效）。
        /// </summary>
        public List<CourtStageCharDef> courtStageDef;
    }

    /// <summary>
    /// 法庭全景动画中单个角色的配置定义。
    /// </summary>
    [System.Serializable]
    public class CourtStageCharDef
    {
        /// <summary>
        /// 角色名称（用于查找立绘和排序）。
        /// </summary>
        public string name;

        /// <summary>
        /// 是否使用自定义坐标覆盖默认值。
        /// </summary>
        public bool useCustomPos;

        /// <summary>
        /// 自定义 X 坐标。
        /// </summary>
        public float x;

        /// <summary>
        /// 自定义 Y 坐标。
        /// </summary>
        public float y;
    }

    /// <summary>
    /// 辩论回合的配置数据。
    /// </summary>
    [System.Serializable]
    public class DebateRoundConfig
    {
        /// <summary>
        /// 本回合的时间限制（秒）。
        /// </summary>
        public float timeLimit;

        /// <summary>
        /// 飘动的句子列表。
        /// </summary>
        public List<FloatingSentenceData> sentences;

        /// <summary>
        /// 点击弱点后弹出的选项列表。
        /// </summary>
        public List<DebateOption> options;
    }

    /// <summary>
    /// 悬浮文字（言弹目标）的数据定义。
    /// </summary>
    [System.Serializable]
    public class FloatingSentenceData
    {
        /// <summary>
        /// 句子的唯一ID
        /// </summary>
        public string id;

        /// <summary>
        /// 显示的文本内容。
        /// 如果 isWeakPoint 为 true，脚本会自动将整句变为弱点。
        /// 如果只想让部分词语成为弱点，请在 JSON 中手动使用 <link="weak">...</link> 包裹该词。
        /// </summary>
        public string text;

        /// <summary>
        /// 是否包含弱点。
        /// true: 脚本会自动处理弱点颜色和点击逻辑。
        /// false: 普通白色文本，不可点击。
        /// </summary>
        public bool isWeakPoint;

        /// <summary>
        /// 击破该弱点所需的证据 ID（如果需要验证证据）。
        /// </summary>
        public string correctEvidenceId; 
    }

    /// <summary>
    /// 辩论选项数据。
    /// </summary>
    [System.Serializable]
    public class DebateOption
    {
        public string id;
        public string text;

        /// <summary>
        /// 是否是正确选项。
        /// </summary>
        public bool isCorrect;

        /// <summary>
        /// 选择正确后跳转到的下一行 ID。
        /// </summary>
        public string nextLineId; 

        /// <summary>
        /// 选择错误时的惩罚时间（秒）。
        /// </summary>
        public int penalty; 
    }
}
