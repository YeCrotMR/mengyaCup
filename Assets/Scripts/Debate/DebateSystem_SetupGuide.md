# 学级裁判系统设置指南 (Debate System Setup Guide)

## 1. 核心概念 (Core Concepts)

本系统模仿《弹丸论破》/《逆转裁判》的辩论环节，核心机制为：
*   **普通文字 (White Text)**: 仅作为视觉信息飘过，点击**无反应**，无惩罚。
*   **弱点文字 (Weak Point)**: 包含矛盾的关键短语，系统会自动识别并渲染为**暗粉色** (`<color=#FF55AA>`)。点击后触发反驳选项。
*   **无缝交互**: 文字直接在屏幕上显示，配合语音自动播放，不需要进入额外的战斗界面。
*   **自动播放循环**: 辩论环节会自动循环播放，直到玩家击破弱点。

## 2. 场景搭建 (Scene Setup)

### 2.1 管理器设置
建议创建空物体 `SystemManagers`，挂载以下所有核心脚本（符合单例模式）：
- `GameManager.cs` (全局状态)
- `TextManager.cs` (资源加载)
- `TreatmentManager.cs` (剧本导演)
- `DebateManager.cs` (辩论逻辑)
- `UIManager.cs` (UI控制)
- `ButtonManager.cs` (统一按钮管理)
- `AudioManager.cs` (音频管理)
- `CourtStageController.cs` (法庭全景卷轴) [新增]

### 2.2 UI 层级结构 (Canvas)
在 Canvas 下创建以下结构，并确保 `UIManager` 脚本引用了对应对象：

```text
Canvas
├── SceneBG (Image)                  <-- 对应 UIManager.sceneBG
├── CourtStage (GameObject)          <-- [新增] 对应 UIManager.courtStageController
│   ├── BG_Scroll (Transform)        <-- 卷轴背景父物体
│   └── Camera (Camera)              <-- 专用摄像机
├── MenuPanel (Panel)                <-- 对应 UIManager.menuPanel
├── DialogPanel (Panel)              <-- 对应 UIManager.dialoguePanel
│   ├── LeftMask/LeftShow (Image)    <-- 对应 UIManager.leftShow
│   ├── CenterMask/CenterShow (Image)<-- 对应 UIManager.centerShow
│   ├── RightMask/RightShow (Image)  <-- 对应 UIManager.rightShow
│   ├── SpeakerName (TMP)            <-- 对应 UIManager.speakerName
│   ├── DialogText (TMP)             <-- 对应 UIManager.dialogText
│   ├── DialogButton (Button)        <-- 全屏透明按钮，绑定 ButtonManager.OnClickButton
│   ├── AutoButton (Button)          <-- 自动播放按钮，绑定 ButtonManager.OnClickButton
│   ├── NextLineIcon (Image)         <-- 挂载 UIAlphaBlink.cs 实现闪烁
│   ├── MenuButton (Button)          <-- 菜单按钮，绑定 ButtonManager.OnClickButton
│   └── DialogChoice (Panel)         <-- 对应 UIManager.dialogueOptionsPanel
│       └── ChoiceGroup (VLG)        <-- Vertical Layout Group，存放选项按钮
│           ├── Top (Button)         <-- 预置选项按钮
│           ├── Middle (Button)      <-- 预置选项按钮
│           └── Bottom (Button)      <-- 预置选项按钮
├── DebatePanel (Panel)              <-- 对应 UIManager.debatePanel
│   ├── DebateLeftShow (Image)       <-- 对应 UIManager.debateLeftShow
│   ├── DebateRightShow (Image)      <-- 对应 UIManager.debateRightShow
│   ├── DebateTextLeft (TMP)         <-- 对应 UIManager.debateTextLeft (左侧文本框)
│   ├── DebateTextRight (TMP)        <-- 对应 UIManager.debateTextRight (右侧文本框)
│   └── TrialChoice (Panel)          <-- 对应 UIManager.debateOptionsPanel
│       └── ChoiceGroup (VLG)        <-- Vertical Layout Group，存放选项按钮
│           ├── Top (Button)         <-- 预置选项按钮 (映射到倒数第2个)
│           ├── Middle (Button)      <-- 预置选项按钮 (映射到倒数第3个)
│           ├── Bottom (Button)      <-- 预置选项按钮 (映射到倒数第4个)
│           └── Cancel (Button)      <-- 返回按钮 (固定为最后一个)，点击恢复辩论
└── OptionsPanel (Panel)             <-- (旧版兼容，可移除)
```

**关键点**:
*   **选项面板分离**: 对话和辩论模式分别使用独立的选项面板 (`DialogChoice` 和 `TrialChoice`)。
*   **按钮复用**: 不再实例化 Prefab，而是直接复用 `ChoiceGroup` 下预置的按钮。
*   **返回按钮**: 在 `TrialChoice` 下的最后一个按钮（如 `Cancel`）会被自动绑定为“返回/取消”功能，点击后恢复时间流逝。
*   所有文本组件必须使用 **TextMeshPro - Text (UI)** 以支持 `<link>` 标签。

## 3. 预制体制作 (Prefab Creation)

### 3.1 悬浮文字 (FloatingText) [已弃用]
*注：新版逻辑使用固定文本框 (`DebateTextLeft`/`DebateTextRight`)，此部分仅作兼容性保留。*

### 3.2 选项按钮设置 (Option Buttons Setup)
虽然现在不再使用 Prefab 动态生成，但场景中的 `Top`, `Middle`, `Bottom` 按钮需满足以下配置：
1.  **组件**: 物体必须包含 `Button` 组件。
2.  **文本**: 子对象必须包含 `TextMeshPro - Text (UI)` 组件，用于显示选项内容。
3.  **布局**: 将这些按钮放置在 `ChoiceGroup` (含 Vertical Layout Group) 下。
4.  **命名**: 建议命名为 `Top`, `Middle`, `Bottom` 以便管理（脚本通过遍历获取，名字不强制，但顺序重要）。
5.  **返回按钮**: 辩论选项面板 (`TrialChoice`) 中的返回按钮必须命名为包含 "Cancel" 或 "Back" 的名称（例如 `Cancel`），且必须是 `ChoiceGroup` 的**最后一个子物体**。

### 3.3 通用按钮配置 (General Buttons)
以下按钮需要绑定到 `ButtonManager.OnClickButton` 方法，并通过按钮的 GameObject 名称来区分功能：

1.  **AutoButton (自动播放)**
    *   **名称**: `AutoButton`
    *   **功能**: 切换自动播放模式。
    *   **子对象**: 包含 Text 和 Image，脚本会自动切换 Image Sprite 和 Text 位置。

2.  **DialogButton (全屏点击)**
    *   **名称**: `DialogButton`
    *   **位置**: 通常作为 DialogPanel 的全屏背景（透明）。
    *   **功能**:
        *   当打字机正在打字时：点击立即显示全句。
        *   当文字显示完毕时：点击播放下一句剧情。

3.  **MenuButton (菜单)**
    *   **名称**: `MenuButton`
    *   **功能**: 显示/隐藏菜单面板 (`MenuPanel`)。

## 4. 剧本配置指南 (JSON Configuration)

文件路径: `Assets/StreamingAssets/ActXX_ChapterXX_TrialXX.json`

### 4.1 基础格式 (ScriptLine)
```json
{
    "id": "line_01",
    "type": "dialogue",       // 类型: dialogue (普通对话), debate (辩论), command (指令)
    "speaker": "角色名",
    "text": "对话内容",
    "portrait": "PortraitName", // 立绘文件名 (Resources/Image/Character/...)
    "position": "center",       // 立绘位置: left, center, right
    "voice": "VoiceFileName",   // 语音文件名 (自动拼接路径)
    "debateConfig": { ... }     // 仅 type="debate" 时有效
}
```

### 4.2 辩论配置 (DebateConfig)
```json
{
    "timeLimit": 300,
    "sentences": [
        {
            "id": "s1",
            "text": "凶手就在<link=\"weak\">那个人</link>中间！", // 必须手动添加 link 标记
            "isWeakPoint": true // 标记该句包含弱点
        }
    ],
    "options": [
        {
            "id": "opt_1",
            "text": "选项文本",
            "isCorrect": true,  // 是否正确选项
            "nextLineId": "TargetLineID", // 跳转目标 (答对跳转，答错若配置则跳转错误剧情)
            "penalty": 10       // 错误惩罚时间
        }
    ]
}
```
*注：移除了 `speed`, `trajectory` 等物理运动参数。*

### 4.3 指令系统 (Command System)
支持通过 `type: "command"` 执行特殊操作：

*   **SwitchCharacter / AddCharacter**: 切换或添加立绘
    *   `parameters`: `["position", "PortraitName"]`
*   **RemoveCharacter**: 移除立绘
    *   `parameters`: `["position"]`
*   **TurnBg**: 切换背景
    *   `parameters`: `["BackgroundName"]`
*   **ShowChoice**: 显示选项分支 (支持跳转)
    *   `parameters`: `["选项文本|跳转ID", "选项文本"]` (如果无ID则默认下一行)
    *   示例: `["同意|Line_005", "反对|Line_010"]`
    *   **注意**: 此指令非阻塞，通常需要配合 `Jump` 或 `LoadNextScript` 使用。
*   **LoadNextScript**: 加载下一个 JSON 脚本 [新增]
    *   `parameters`: `["NextScriptFileName.json"]`
*   **Jump**: 跳转到当前脚本内的指定行 [新增]
    *   `parameters`: `["TargetLineID"]`
*   **PlayBGM**: 播放背景音乐
    *   `parameters`: `["BGMIndex"]` (索引对应 AudioManager 中的 BGMList)
*   **SwitchBGM**: 切换背景音乐
    *   `parameters`: `["BGMIndex"]`
*   **StopBGM**: 停止背景音乐
    *   `parameters`: []
*   **InitCourtStage / StopCourtStage**: 开启/关闭法庭全景卷轴动画 [新增]

### 4.4 音频路径规则
*   **Voice**: 
    *   如果在 JSON 中只填写文件名 (如 `0101Trial01_Sherry001`)，系统会自动拼接为：`Audio/Voice/{ParentChapterID}/{ChapterID}/{FileName}`。
    *   如果在 JSON 中填写包含 `/` 的路径，则直接使用该路径。
*   **BGM/SFX**: 在 `AudioManager` 的 Inspector 中配置 `BGMList`，通过索引调用。

## 5. 功能特性
*   **自动播放**: 点击 `AutoButton` 切换自动播放模式，按钮状态会自动更新。
*   **打字机效果**: 对话文本逐字显示，支持点击 `DialogButton` 或屏幕任意位置加速显示整句。
*   **立绘管理**: 支持多立绘同屏，支持原生大小 (`SetNativeSize`) 并自动缩放（除以2）以适配高清素材。
*   **UI 闪烁**: `NextLineIcon` 使用 `UIAlphaBlink` 脚本实现呼吸闪烁效果。
*   **富文本交互**: 辩论中支持 `<link="weak">` 标签检测，点击触发反驳。
*   **动态选项布局**: 辩论选项根据数量自动倒序排列，始终保留返回按钮。

---

## 6. 系统维护与重构说明 (2026-02-12)

### 6.1 已移除组件
*   **FloatingText**: 原有的悬浮文字系统已完全移除。辩论文本现在统一通过 `UIManager` 的 `DebateTextLeft` 和 `DebateTextRight` 进行固定位置渲染，极大简化了场景层级和点击判定逻辑。

### 6.2 架构变更
*   **全局单例**: 所有的 Manager 现已迁移至 `Singleton<T>` 基类，确保了全局唯一性及方便的跨脚本访问。
*   **UI 状态托管**: `AutoButton` 的切图和文本位移逻辑已从 `ButtonManager` 迁移至 `UIManager.UpdateAutoButtonState`。在搭建新场景时，只需确保按钮对象符合 Section 3.3 的结构，UI 逻辑将自动处理。

### 6.3 资源路径优化
*   **Voice 路径自动化**: `TreatmentManager` 内部封装了路径拼接逻辑，剧本编写者仅需在 JSON 中提供文件名。系统会根据当前章节 ID 自动匹配 `Resources/Audio/Voice/` 下的子文件夹。
