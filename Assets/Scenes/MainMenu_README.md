# MainMenu场景设置说明

## 已完成的工作

1. **MainMenuController.cs** - 主菜单控制器脚本已创建
   - 位置：`Assets/Scripts/UI/MainMenuController.cs`
   - 功能：管理4个按钮的点击事件
   - 预留了PNG图片接口，当前使用文字版本

2. **MainMenu.unity场景** - 已添加基础UI结构
   - Canvas（UI画布）
   - EventSystem（事件系统）
   - Background（背景图，需要在Inspector中设置图片）
   - 4个按钮：
     - StartGameButton（开始游戏）
     - LoadGameButton（读取存档）
     - SettingsButton（设置）
     - QuitGameButton（退出游戏）

## 需要在Unity编辑器中完成的设置

### 1. 设置背景图
- 选择Canvas下的Background对象
- 在Inspector中找到Image组件
- 将背景图片拖拽到Sprite字段

### 2. 配置MainMenuController脚本
- 选择Canvas对象
- 在Inspector中找到MainMenuController组件
- 将4个按钮拖拽到对应的字段：
  - startGameButton → StartGameButton
  - loadGameButton → LoadGameButton
  - settingsButton → SettingsButton
  - quitGameButton → QuitGameButton
- 将4个Text组件拖拽到对应的字段：
  - startGameText → StartGameButton/Text
  - loadGameText → LoadGameButton/Text
  - settingsText → SettingsButton/Text
  - quitGameText → QuitGameButton/Text

### 3. 设置场景名称（可选）
- 在MainMenuController组件中
- 修改gameSceneName为你要加载的游戏场景名称（默认：Adv）
- 或修改gameSceneIndex为场景索引（默认：1）

### 4. 设置按钮文字（如果需要修改）
- 选择各个按钮下的Text子对象
- 在TextMeshProUGUI组件中修改m_text字段

## 按钮功能说明

1. **开始游戏** - 加载游戏场景（使用FadeController进行淡入淡出）
2. **读取存档** - 预留接口，需要实现存档系统
3. **设置** - 加载Setting场景
4. **退出游戏** - 退出应用程序

## PNG图片接口说明

MainMenuController脚本已预留PNG图片接口：

- `startGameImage`, `loadGameImage`, `settingsImage`, `quitGameImage` - Image组件引用
- `startGameSprite`, `loadGameSprite`, `settingsSprite`, `quitGameSprite` - Sprite资源引用
- `SwitchToImageMode()` - 切换到PNG图片模式的方法

当需要使用PNG图片时：
1. 在Inspector中为每个按钮的Image组件设置Sprite
2. 调用`SwitchToImageMode()`方法切换到图片模式
3. 文字组件会自动隐藏，图片组件会自动显示

## 注意事项

- 确保项目中已导入TextMeshPro包
- 确保字体资源存在（当前使用LiberationSans SDF）
- 如果场景切换使用FadeController，确保FadeController已正确设置
