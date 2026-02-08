using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 从 Backpack 场景创建背包预制体，用于放入 UI 场景。
/// 菜单：Tools → Create Backpack Prefab
/// </summary>
public static class BackpackPrefabCreator
{
    [MenuItem("Tools/Create Backpack Prefab")]
    public static void CreateBackpackPrefab()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        string backpackPath = "Assets/Scenes/Test/Backpack.unity";

        // 打开 Backpack 场景
        var scene = EditorSceneManager.OpenScene(backpackPath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("[BackpackPrefabCreator] 无法打开 Backpack 场景: " + backpackPath);
            return;
        }

        var manager = GameObject.Find("BackpackManager");
        var canvas = GameObject.Find("Canvas");

        if (manager == null || canvas == null)
        {
            Debug.LogError("[BackpackPrefabCreator] 未找到 BackpackManager 或 Canvas，请确保 Backpack 场景结构正确。");
            return;
        }

        // 创建根物体
        var root = new GameObject("Backpack");
        manager.transform.SetParent(root.transform);
        canvas.transform.SetParent(root.transform);

        string prefabPath = "Assets/Prefabs/Backpack.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        if (prefab != null)
        {
            Debug.Log("[BackpackPrefabCreator] 已创建预制体: " + prefabPath + "\n请将预制体拖入 UI 场景，并在 ExitButton 上勾选 closeAsPanel。");
        }
        else
        {
            Debug.LogError("[BackpackPrefabCreator] 创建预制体失败。");
        }

        // 恢复场景：把 BackpackManager 和 Canvas 移回根
        manager.transform.SetParent(null);
        canvas.transform.SetParent(null);
        Object.DestroyImmediate(root);

        EditorSceneManager.SaveScene(scene);

        // 恢复之前打开的场景
        if (!string.IsNullOrEmpty(activeScene) && activeScene != backpackPath)
            EditorSceneManager.OpenScene(activeScene, OpenSceneMode.Single);
    }
}
