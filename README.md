# 萌芽杯游戏框架 (MengyaCup Game Framework)

这是一个基于 Unity 的 2D 探索解谜游戏框架，集成了背包管理、剧情对话、场景交互与 UI 状态管理等核心功能。

## 🌟 核心功能

### 1. 背包系统 (Backpack System)
用于管理游戏中的核心数据，支持自动去重与分类显示。
- **三类信息**：
  - **人物 (Character)**：记录人物证词，支持多角色管理。
  - **疑点 (Suspicion)**：探索阶段获得的关键道具/线索。
  - **结论 (Conclusion)**：剧情推理阶段得出的结论。
- **自动解锁**：获得首个疑点时自动解锁“疑点”标签；剧情推进后解锁“结论”标签。
- **UI 交互**：支持列表查看、详情展示（含特写图）及“出示证物”模式。

### 2. 对话系统 (Dialogue System)
功能完善的剧情演出模块。
- **打字机效果**：逐字显示文本，支持点击跳过当前句。
- **分支选项**：支持多分支剧情选择 (`DialogueChoice`)，影响后续对话。
- **对话记录**：自动记录历史对话内容 (`DialogueLogManager`)。

### 3. 探索与交互 (Interaction System)
通过挂载 `SearchableItem` 脚本，快速将场景物体转化为可交互对象。
- **三种交互模式**：
  1. **DescribeInDialog**：点击后显示一段描述性对话。
  2. **TriggerChoice**：点击后触发带有选项的剧情对话。
  3. **KeyEvidenceToBackpack**：点击后获得“关键证物”，自动存入背包并弹出提示。
- **一次性交互**：支持配置 `One Time Only`，防止重复触发。

### 4. UI 架构 (UI Manager)
基于栈 (Stack) 的 UI 管理系统，确保界面层级正确。
- **状态管理**：打开 UI 时自动暂停游戏时间 (`Time.timeScale = 0`) 并静音。
- **导航控制**：统一处理 ESC 键逻辑，支持层级回退。
- **场景兼容**：跨场景单例设计，支持 `DontDestroyOnLoad`。

---

## 📂 项目结构

```
Assets/Scripts/
├── Bag/                # 背包系统核心代码
│   ├── BackpackManager.cs    # 数据与逻辑管理 (单例)
│   ├── BackpackUI.cs         # 界面显示逻辑
│   └── ...
├── Chat/               # 对话系统
│   ├── DialogueSystem.cs     # 对话控制器
│   ├── DialogueLine.cs       # 数据结构定义
│   └── ...
├── Serach/             # 搜索/交互系统 (拼写待修正: Search)
│   └── SearchableItem.cs     # 场景交互脚本
├── UI/                 # 通用 UI 框架
│   ├── UIManager.cs          # UI 栈管理器
│   └── ...
└── InfoType/           # 枚举定义
```

---

## 🚀 快速上手

### 1. 配置可交互物品
1. 在场景中创建一个物体（Sprite 或 3D 模型）。
2. 添加 `Box Collider 2D` (2D) 或 `Box Collider` (3D)。
3. 挂载 `SearchableItem` 脚本。
4. 在 Inspector 中选择 **Interaction Type**：
   - 若选 **Key Evidence To Backpack**，需填写 `Evidence Id`、标题、描述及特写图。

### 2. 使用背包系统
- **添加证物**：
  ```csharp
  BackpackManager.Instance.AddEvidence("id_01", "带血的刀", "在现场发现的...", spriteIcon);
  ```
- **出示证物**：
  在剧情逻辑中调用 `BackpackUI.OpenForPresentingEvidence()` 即可打开背包供玩家选择。

### 3. 启动对话
```csharp
var lines = new DialogueLine[] {
    new DialogueLine { text = "你好，侦探。", speakerName = "NPC" }
};
DialogueSystem.Instance.SetDialogue(lines);
DialogueSystem.Instance.StartDialogue();
```

---

## 📝 开发备注
- **UI 资源**：位于 `Assets/Image/UI/`，请确保预制体引用丢失时重新绑定。
- **场景设置**：新场景需确保包含 `UIManager` 和 `BackpackManager` 预制体/实例。
