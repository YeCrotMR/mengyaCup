using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("按钮引用")]
    public Button startGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button quitGameButton;

    [Header("按钮文字组件（当前使用）")]
    public TextMeshProUGUI startGameText;
    public TextMeshProUGUI loadGameText;
    public TextMeshProUGUI settingsText;
    public TextMeshProUGUI quitGameText;

    [Header("按钮图片组件（预留PNG接口）")]
    public Image startGameImage;
    public Image loadGameImage;
    public Image settingsImage;
    public Image quitGameImage;

    [Header("按钮图片资源（预留PNG接口）")]
    public Sprite startGameSprite;
    public Sprite loadGameSprite;
    public Sprite settingsSprite;
    public Sprite quitGameSprite;

    [Header("场景设置")]
    public string gameSceneName = "Adv"; // 开始游戏要加载的场景名称
    public int gameSceneIndex = 1; // 或者使用场景索引

    private void Start()
    {
        // 初始化按钮事件
        SetupButtons();
        
        // 初始化按钮显示（当前使用文字版本）
        InitializeButtonDisplay();
    }

    void SetupButtons()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
        
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnLoadGameClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }

    void InitializeButtonDisplay()
    {
        // 当前使用文字版本，隐藏图片组件
        if (startGameImage != null) startGameImage.enabled = true;
        if (loadGameImage != null) loadGameImage.enabled = true;
        if (settingsImage != null) settingsImage.enabled = true;
        if (quitGameImage != null) quitGameImage.enabled = true;

        // 确保文字组件启用
        if (startGameText != null) startGameText.enabled = true;
        if (loadGameText != null) loadGameText.enabled = true;
        if (settingsText != null) settingsText.enabled = true;
        if (quitGameText != null) quitGameText.enabled = true;
    }

    /// <summary>
    /// 切换到PNG图片模式（预留接口）
    /// </summary>
    public void SwitchToImageMode()
    {
        // 隐藏文字
        if (startGameText != null) startGameText.enabled = false;
        if (loadGameText != null) loadGameText.enabled = false;
        if (settingsText != null) settingsText.enabled = false;
        if (quitGameText != null) quitGameText.enabled = false;

        // 显示图片并设置Sprite
        if (startGameImage != null && startGameSprite != null)
        {
            startGameImage.enabled = true;
            startGameImage.sprite = startGameSprite;
        }
        if (loadGameImage != null && loadGameSprite != null)
        {
            loadGameImage.enabled = true;
            loadGameImage.sprite = loadGameSprite;
        }
        if (settingsImage != null && settingsSprite != null)
        {
            settingsImage.enabled = true;
            settingsImage.sprite = settingsSprite;
        }
        if (quitGameImage != null && quitGameSprite != null)
        {
            quitGameImage.enabled = true;
            quitGameImage.sprite = quitGameSprite;
        }
    }

    void OnStartGameClicked()
    {
        Debug.Log("开始游戏 - 加载场景: " + gameSceneName);
        
        // 检查场景是否存在
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            if (!IsSceneInBuildSettings(gameSceneName))
            {
                Debug.LogError($"场景 '{gameSceneName}' 未添加到Build Settings中！\n" +
                    "请按照以下步骤添加：\n" +
                    "1. 打开菜单 File -> Build Settings\n" +
                    "2. 将场景文件拖拽到Scenes In Build列表中\n" +
                    "3. 或者点击Add Open Scenes按钮添加当前打开的场景");
                return;
            }
        }
        
        // 使用FadeController进行场景切换（如果存在）
        if (FadeController.Instance != null)
        {
            // 优先使用场景名称
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                FadeController.Instance.FadeAndLoadScene(gameSceneName);
            }
            else
            {
                FadeController.Instance.FadeAndLoadScene(gameSceneIndex);
            }
        }
        else
        {
            // 如果没有FadeController，直接加载场景
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneIndex);
            }
        }
    }
    
    /// <summary>
    /// 检查场景是否在Build Settings中
    /// </summary>
    bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    void OnLoadGameClicked()
    {
        Debug.Log("读取存档");
        // TODO: 实现读取存档功能
        // 这里可以打开存档选择界面或直接加载最新存档
    }

    void OnSettingsClicked()
    {
        Debug.Log("打开设置");
        // TODO: 实现设置功能
        // 这里可以打开设置界面
        SceneManager.LoadScene("Setting");
    }

    void OnQuitGameClicked()
    {
        Debug.Log("退出游戏");
        
#if UNITY_EDITOR
        // 如果在编辑器中，停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 如果是打包后运行，退出应用
        Application.Quit();
#endif
    }
}
