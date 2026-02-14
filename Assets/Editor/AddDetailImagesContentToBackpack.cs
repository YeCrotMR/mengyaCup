using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 在背包预制体中添加 DetailImagesContent 节点，方便在编辑器中调整图片位置。
/// 菜单：Tools → Add DetailImagesContent to Backpack
/// </summary>
public static class AddDetailImagesContentToBackpack
{
    [MenuItem("Tools/Add DetailImagesContent to Backpack")]
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/Backpack.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("[AddDetailImagesContent] 未找到背包预制体: " + prefabPath);
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError("[AddDetailImagesContent] 实例化预制体失败");
            return;
        }

        var backpackUI = instance.GetComponentInChildren<BackpackUI>(true);
        if (backpackUI == null)
        {
            Debug.LogError("[AddDetailImagesContent] 未找到 BackpackUI");
            Object.DestroyImmediate(instance);
            return;
        }

        var detailArea = backpackUI.transform.Find("DetailArea");
        if (detailArea == null)
        {
            Debug.LogError("[AddDetailImagesContent] 未找到 DetailArea");
            Object.DestroyImmediate(instance);
            return;
        }

        var itemDescArea = detailArea.Find("ItemDescArea");
        if (itemDescArea == null)
        {
            Debug.LogError("[AddDetailImagesContent] 未找到 ItemDescArea");
            Object.DestroyImmediate(instance);
            return;
        }

        var existing = itemDescArea.Find("DetailImagesContent");
        if (existing != null)
        {
            Debug.Log("[AddDetailImagesContent] DetailImagesContent 已存在，已更新 BackpackUI 引用");
            var so = new SerializedObject(backpackUI);
            so.FindProperty("detailImagesContent").objectReferenceValue = existing;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            var go = new GameObject("DetailImagesContent");
            go.transform.SetParent(itemDescArea, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 50);
            rt.sizeDelta = new Vector2(0, 100);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var so = new SerializedObject(backpackUI);
            so.FindProperty("detailImagesContent").objectReferenceValue = go.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[AddDetailImagesContent] 已添加 DetailImagesContent 到背包预制体");
        }

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);

        AssetDatabase.Refresh();
        Debug.Log("[AddDetailImagesContent] 完成。可在 Hierarchy 中选中 Backpack 预制体 → DetailArea → ItemDescArea → DetailImagesContent 调整位置。");
    }
}
