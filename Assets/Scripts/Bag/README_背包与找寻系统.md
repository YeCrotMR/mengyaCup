# 背包系统 + 找寻系统 + 信息种类 使用说明

## 一、找寻系统（探索阶段互动物品）

### 1. 用法
- 在**探索阶段**进入具体房间后，在需要互动的物体上添加组件 **SearchableItem**。
- 为该物体挂上 **Collider2D**（2D 项目）或 **Collider**（3D），否则点击无法检测。
- 玩家点击该物体后，根据你在 Inspector 里配置的「互动类型」产生三种情况之一。

### 2. 三种互动类型（Inspector 中选 SearchableItem → Interaction Type）

| 类型 | 说明 | 需配置项 |
|------|------|----------|
| **DescribeInDialog** | 在对话框里陈述物品 | describeText、可选 describePortrait |
| **TriggerChoice** | 触发选择事件（分支） | choiceDialogueLines（DialogueLine 数组，带 hasChoices/choices） |
| **KeyEvidenceToBackpack** | 获得关键信息（特写图）并记录至背包 | evidenceId、evidenceTitle、evidenceDescription、evidenceCloseUpImage |

### 3. 可选
- **One Time Only**：勾选后该物品只能被点击一次。

### 4. 疑点物品：从点击到入背包（做一个具体物品）

下面以「桌上的一封信」为例，做**一个**点击后加入背包疑点的物品。

#### 步骤 1：准备可点击的物体

- 在探索场景里放一个代表「信」的物体（例如一张 Sprite、一个 3D 模型、或一个带子物体的空物体）。
- 确保玩家能看到并点击到它（位置、层级正常即可）。

#### 步骤 2：挂碰撞体（必须）

- 选中该物体，**Add Component**：
  - **2D 项目**：加 **Box Collider 2D**（或 Polygon Collider 2D 等），用绿色线框盖住「信」的可见范围。
  - **3D 项目**：加 **Box Collider**（或 Sphere/Mesh Collider），同样盖住可点击范围。
- 不挂 Collider 的话，`SearchableItem` 的点击（OnMouseDown）不会触发。

#### 步骤 3：挂 SearchableItem 并选「入背包」

- 同一物体上 **Add Component** → **Searchable Item**。
- 在 Inspector 里把 **Interaction Type** 选成 **Key Evidence To Backpack**。

#### 步骤 4：填疑点内容（证物信息）

在 **SearchableItem** 的「关键信息入背包」区域填写：

| 字段 | 示例（一封信） | 说明 |
|------|----------------|------|
| **Evidence Id** | `letter_01` | 唯一 ID，不能和别的证物重复 |
| **Evidence Title** | 桌上的信 | 背包列表和详情里显示的标题 |
| **Evidence Description** | 一封从桌上找到的信，内容似乎与案件有关…… | 背包里点开该疑点后显示的详细描述 |
| **Evidence Close Up Image** | 拖入「信」的特写 Sprite | 可选；不拖则无图，背包里仍会显示标题和描述 |

- 图片需先设为 **Sprite**：在 Project 里选中图片 → Inspector → **Texture Type** 选 **Sprite (2D and UI)**，再拖到 **Evidence Close Up Image**。

#### 步骤 5：可选设置

- **One Time Only**：勾选后，玩家只能点击一次，再次点击无效（适合「拿走就没了」的证物）。

#### 流程小结（点击后发生什么）

1. 玩家在探索场景**点击**这封信（带 Collider 的物体）。
2. `SearchableItem` 检测到点击，类型为 `KeyEvidenceToBackpack`，执行 `RunKeyEvidence()`。
3. 调用 `BackpackManager.Instance.AddEvidence("letter_01", "桌上的信", "一封从桌上找到的信……", 特写Sprite)`，把该条加入背包。
4. 同时弹出对话框：「获得了关键信息：「桌上的信」已记录至背包。」（若填了特写图，对话框会带该图）。
5. 打开背包 → 切到**疑点**标签 → 列表中会出现「桌上的信」，点击可看详情和特写图。

按上述步骤即可完成**一个**疑点物品从点击到入背包的完整流程；其他疑点物品重复步骤 1～5，换不同的 Id/标题/描述/图即可。

---

## 二、背包系统

### 1. 场景与预制体
- 在场景中放一个**空物体**，挂上 **BackpackManager**（单例，建议 DontDestroyOnLoad 已在该脚本内处理）。
- 再做一个**背包面板**（Canvas 下），挂上 **BackpackUI**，并拖好下面说的引用。

### 2. BackpackManager 配置
- **Initial Character Ids**：4 个人物的 ID，如 `char_1` ~ `char_4`。
- **Initial Character Names**：4 个显示名，与上面一一对应。
- **Initial Character Sprites**：4 个人物头像（Sprite），与上面 ID 一一对应；在 Inspector 中把头像图拖入数组对应元素即可，可不设或数量不足则该人物无图。
- **Initial Character Detail Images**：4 个人物详情描述中的附加图片（SpriteArray 数组，每个元素是一组 Sprite）；可为每个人物配置多张图，显示在详情描述区域下方，可不设或数量不足则无图。
  开局会自动在「人物」标签下生成这 4 人；证词可通过 `AddTestimony(..., characterId)` 关联到对应人物。

### 3. BackpackUI 配置
- **backpackPanel**：背包根节点（整个面板）。
- **右侧标签**：tabCharacter / tabSuspicion / tabConclusion 三个 Button；可选 tabXxxLockHint 未解锁时显示的锁提示。
- **列表**：itemListContent（ScrollView 的 Content）、itemSlotPrefab（推荐 `BackpackItemSlot.prefab`，含图片 Icon + 名称 Text）。
- **详情**：detailPanel、detailTitle、detailDescription、detailImage；可选 detailImagesContent（详情描述中的附加图片容器，不设则自动创建）。
- **uiManager**：拖场景中的 UIManager，这样打开/关闭背包会走 PushUI/PopUI，与 ESC 主页一致。

### 4. 背包界面背景与退出按钮
- **背包背景**：背包面板根节点上的 Image 组件设为灰黑色（如 r:0.2, g:0.2, b:0.22, a:1）。
- **退出按钮**：在背包面板**右上角**放置一个 Button，挂上 **BackpackExitButton** 脚本。
  - **预制体模式**（背包在 UI 场景内）：勾选 `closeAsPanel`，点击后关闭背包返回主页面。
  - **独立场景模式**：不勾选 `closeAsPanel`，点击后加载 `targetSceneName` 场景。

### 5. 预制体方案与布局设置
1. 菜单 **Tools → Setup Backpack Layout**：按设计图完善背包布局（左物品图、中描述、右 tag 按钮、底物品栏），自动绑定引用。
2. 菜单 **Tools → Create Backpack Prefab**，会在 `Assets/Prefabs/Backpack.prefab` 创建预制体。
3. 打开 **UI.unity**，将 **Backpack** 预制体从 Project 拖入 Hierarchy（可放在 Canvas 同级或任意位置）。
4. 选中预制体实例下的 **ExitButton**，在 Inspector 中勾选 **Close As Panel**。
5. （可选）选中 **BackpackUI**，将场景中的 **UIManager** 拖到 `uiManager` 字段；若未拖则会在运行时自动查找。
6. 点击主页面背包按钮即可打开背包，点击右上角退出按钮关闭背包。

### 6. 主页面背包按钮
- 在主页面（ESC 弹出的那个）**左上角**放一个 Button，把背包图标的 **PNG** 设为该 Button 的 Image。
- 在该 Button 上挂 **BackpackButtonOpener**，无需再手动绑定 On Click；脚本会自动在点击时调用 `BackpackUI.Instance.OpenBackpack()`。
- 若希望「出示证物」时自动打开背包，在庭审/出示逻辑里调用：  
  `BackpackUI.OpenForPresentingEvidence();`

### 7. 代码里往背包加内容
- 证物（找寻阶段关键线索）：  
  `BackpackManager.Instance.AddEvidence(id, title, description, image);`
- 证词/重要文本（可关联人物）：  
  `BackpackManager.Instance.AddTestimony(id, title, description, characterId, image);`
- 结论（意识流后）：  
  `BackpackManager.Instance.AddConclusion(id, title, description, image);`
- 更新某条目的详情描述图片（如人物详情中的附加图片）：  
  `BackpackManager.Instance.UpdateItemDetailImages(id, detailImages);`
- 解锁「结论」标签（意识流结束后）：  
  `BackpackManager.Instance.UnlockConclusionCategory();`

---

## 三、信息种类系统（背包内标签）

- 背包页面**右侧**为三个标签：**人物**、**疑点**、**结论**。
- **人物**：开局解锁，默认 4 人；证词通过 `AddTestimony(..., characterId)` 会出现在人物标签下。
- **疑点**：玩家在找寻阶段首次获得任意证物（AddEvidence）时自动解锁。
- **结论**：需要在意识流后调用 `UnlockConclusionCategory()` 解锁；之后用 `AddConclusion` 添加的条目会显示在此标签下。

---

## 四、场景挂载检查清单

- [ ] 场景中有 **BackpackManager**（可挂在常驻 GameObject 上）。
- [ ] 场景中有 **BackpackUI**，并绑定背包面板、三个标签按钮、列表 Content、条目预制体、详情区。
- [ ] 主页面左上角背包按钮挂有 **BackpackButtonOpener**，且背包图标 PNG 已赋给 Button。
- [ ] 探索场景中可点击物品挂有 **SearchableItem** + **Collider2D/Collider**，并选好互动类型与对应配置。
- [ ] DialogueSystem、UIManager 已存在且运行正常（找寻的对话与选项依赖它们）。
