using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneBuildSettingsHelper : EditorWindow
{
    [MenuItem("Tools/添加场景到Build Settings")]
    public static void AddScenesToBuildSettings()
    {
        // 获取所有场景文件
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        
        if (sceneGuids.Length == 0)
        {
            Debug.LogWarning("未找到任何场景文件！");
            return;
        }
        
        // 获取当前Build Settings中的场景
        var scenesInBuild = EditorBuildSettings.scenes.ToList();
        
        int addedCount = 0;
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            // 检查场景是否已经在Build Settings中
            bool alreadyInBuild = scenesInBuild.Any(s => s.path == scenePath);
            
            if (!alreadyInBuild)
            {
                scenesInBuild.Add(new EditorBuildSettingsScene(scenePath, true));
                addedCount++;
                Debug.Log($"已添加场景到Build Settings: {sceneName} ({scenePath})");
            }
            else
            {
                Debug.Log($"场景已在Build Settings中: {sceneName}");
            }
        }
        
        // 更新Build Settings
        EditorBuildSettings.scenes = scenesInBuild.ToArray();
        
        if (addedCount > 0)
        {
            Debug.Log($"成功添加 {addedCount} 个场景到Build Settings！");
        }
        else
        {
            Debug.Log("所有场景都已添加到Build Settings中。");
        }
    }
    
    [MenuItem("Tools/检查MainMenu场景配置")]
    public static void CheckMainMenuConfiguration()
    {
        // 检查Adv场景是否在Build Settings中
        bool advInBuild = false;
        int advIndex = -1;
        
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            string scenePath = EditorBuildSettings.scenes[i].path;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == "Adv")
            {
                advInBuild = true;
                advIndex = i;
                break;
            }
        }
        
        if (advInBuild)
        {
            Debug.Log($"✓ Adv场景已在Build Settings中（索引: {advIndex}）");
        }
        else
        {
            Debug.LogWarning("✗ Adv场景未在Build Settings中！");
            Debug.LogWarning("请使用 Tools -> 添加场景到Build Settings 来添加所有场景");
        }
        
        // 检查MainMenu场景是否在Build Settings中
        bool mainMenuInBuild = false;
        int mainMenuIndex = -1;
        
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            string scenePath = EditorBuildSettings.scenes[i].path;
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == "MainMenu")
            {
                mainMenuInBuild = true;
                mainMenuIndex = i;
                break;
            }
        }
        
        if (mainMenuInBuild)
        {
            Debug.Log($"✓ MainMenu场景已在Build Settings中（索引: {mainMenuIndex}）");
        }
        else
        {
            Debug.LogWarning("✗ MainMenu场景未在Build Settings中！");
        }
        
        // 显示Build Settings窗口
        EditorApplication.ExecuteMenuItem("File/Build Settings...");
    }
}
