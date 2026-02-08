using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 按设计图完善背包 UI 布局：左物品图、中描述、右 tag 按钮、底物品栏。
/// 菜单：Tools → Setup Backpack Layout
/// </summary>
public static class BackpackLayoutSetup
{
    [MenuItem("Tools/Setup Backpack Layout")]
    public static void SetupLayout()
    {
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        string backpackPath = "Assets/Scenes/Test/Backpack.unity";
        var scene = EditorSceneManager.OpenScene(backpackPath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("[BackpackLayoutSetup] 无法打开 Backpack 场景");
            return;
        }

        var backpackUI = Object.FindObjectOfType<BackpackUI>();
        if (backpackUI == null)
        {
            Debug.LogError("[BackpackLayoutSetup] 未找到 BackpackUI");
            return;
        }

        var rt = backpackUI.GetComponent<RectTransform>();
        if (rt == null) return;

        // 获取三个 tag 按钮
        var charBtn = backpackUI.tabCharacter;
        var susBtn = backpackUI.tabSuspicion;
        var conBtn = backpackUI.tabConclusion;
        if (charBtn == null || susBtn == null || conBtn == null)
        {
            Debug.LogError("[BackpackLayoutSetup] 缺少 tag 按钮引用");
            return;
        }

        // 调整 tag 按钮位置到右侧（垂直排列）
        float rightMargin = 80;
        float btnW = 120;
        float btnH = 36;
        float spacing = 10;
        SetRect(charBtn.transform as RectTransform, 1, 1, -rightMargin - btnW / 2, -80, btnW, btnH);
        SetRect(susBtn.transform as RectTransform, 1, 1, -rightMargin - btnW / 2, -80 - btnH - spacing, btnW, btnH);
        SetRect(conBtn.transform as RectTransform, 1, 1, -rightMargin - btnW / 2, -80 - (btnH + spacing) * 2, btnW, btnH);

        // 创建/查找详情区（物品图 + 描述）
        Transform detailArea = rt.Find("DetailArea");
        if (detailArea == null)
        {
            var go = new GameObject("DetailArea");
            go.transform.SetParent(rt, false);
            var detailRt = go.AddComponent<RectTransform>();
            detailArea = go.transform;
            SetRect(detailRt, 0, 0.35f, 0, 0, 1, 0.65f);
        }

        // 物品图片区（左）
        Transform imgArea = detailArea.Find("ItemImageArea");
        if (imgArea == null)
        {
            var go = new GameObject("ItemImageArea");
            go.transform.SetParent(detailArea, false);
            var imgRt = go.AddComponent<RectTransform>();
            imgArea = go.transform;
            SetRect(imgRt, 0, 0, 20, 20, 0.25f, 1);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.17f, 0.9f);
        }

        // detailImage 放在 ItemImageArea
        Transform detailImgTrans = imgArea.Find("DetailImage");
        Image detailImage;
        if (detailImgTrans == null)
        {
            var go = new GameObject("DetailImage");
            go.transform.SetParent(imgArea, false);
            var imgRt = go.AddComponent<RectTransform>();
            SetRect(imgRt, 0.1f, 0.1f, 0, 0, 0.8f, 0.8f);
            detailImage = go.AddComponent<Image>();
            detailImage.color = new Color(0.3f, 0.3f, 0.32f, 0.8f);
            detailImage.preserveAspect = true;
        }
        else
        {
            detailImage = detailImgTrans.GetComponent<Image>();
        }

        // 物品描述区（中）
        Transform descArea = detailArea.Find("ItemDescArea");
        if (descArea == null)
        {
            var go = new GameObject("ItemDescArea");
            go.transform.SetParent(detailArea, false);
            var descRt = go.AddComponent<RectTransform>();
            descArea = go.transform;
            SetRect(descRt, 0.26f, 0, 10, 20, 0.58f, 1); // 留出右侧给 tag 按钮
        }

        Transform detailTitleTrans = descArea.Find("DetailTitle");
        Text detailTitle;
        if (detailTitleTrans == null)
        {
            var go = new GameObject("DetailTitle");
            go.transform.SetParent(descArea, false);
            var titleRt = go.AddComponent<RectTransform>();
            SetRect(titleRt, 0, 0.7f, 10, 5, 1, 0.25f);
            detailTitle = go.AddComponent<Text>();
            detailTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            detailTitle.fontSize = 18;
            detailTitle.color = Color.white;
        }
        else
        {
            detailTitle = detailTitleTrans.GetComponent<Text>();
        }

        Transform detailDescTrans = descArea.Find("DetailDescription");
        Text detailDescription;
        if (detailDescTrans == null)
        {
            var go = new GameObject("DetailDescription");
            go.transform.SetParent(descArea, false);
            var descRt = go.AddComponent<RectTransform>();
            SetRect(descRt, 0, 0, 10, 10, 1, 0.65f);
            detailDescription = go.AddComponent<Text>();
            detailDescription.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            detailDescription.fontSize = 14;
            detailDescription.color = new Color(0.9f, 0.9f, 0.9f, 1);
            detailDescription.alignment = TextAnchor.UpperLeft;
            detailDescription.supportRichText = true;
            detailDescription.horizontalOverflow = HorizontalWrapMode.Wrap;
            detailDescription.verticalOverflow = VerticalWrapMode.Truncate;
        }
        else
        {
            detailDescription = detailDescTrans.GetComponent<Text>();
        }

        // 物品栏（底）ScrollView
        Transform itemListArea = rt.Find("ItemListArea");
        if (itemListArea == null)
        {
            var go = new GameObject("ItemListArea");
            go.transform.SetParent(rt, false);
            var areaRt = go.AddComponent<RectTransform>();
            itemListArea = go.transform;
            SetRect(areaRt, 0, 0, 20, 20, 0.85f, 0.32f); // 不延伸到 tag 按钮下方
        }

        ScrollRect scrollRect;
        Transform content;
        if (!itemListArea.GetComponent<ScrollRect>())
        {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(itemListArea, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            SetRect(scrollRt, 0, 0, 0, 0, 1, 1);

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.AddComponent<RectTransform>();
            SetRect(viewportRt, 0, 0, 0, 0, 1, 1);
            viewportGo.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.5f);
            var mask = viewportGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 0.5f);
            contentRt.anchorMax = new Vector2(1, 0.5f);
            contentRt.pivot = new Vector2(0, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);
            content = contentGo.transform;

            var hlg = contentGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
        }
        else
        {
            scrollRect = itemListArea.GetComponentInChildren<ScrollRect>();
            content = scrollRect.content;
            // 确保 Content 使用横向布局（槽位左右排列）
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg);
                var hlg = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8;
                hlg.padding = new RectOffset(5, 5, 5, 5);
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }
            var csf = content.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
        }

        // 绑定 BackpackUI
        var so = new SerializedObject(backpackUI);
        so.FindProperty("detailPanel").objectReferenceValue = detailArea.gameObject;
        so.FindProperty("detailImage").objectReferenceValue = detailImage;
        so.FindProperty("detailTitle").objectReferenceValue = detailTitle;
        so.FindProperty("detailDescription").objectReferenceValue = detailDescription;
        so.FindProperty("itemListContent").objectReferenceValue = content;

        var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BackpackItemSlot.prefab");
        if (slotPrefab != null)
            so.FindProperty("itemSlotPrefab").objectReferenceValue = slotPrefab;

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (!string.IsNullOrEmpty(activeScene) && activeScene != backpackPath)
            EditorSceneManager.OpenScene(activeScene, OpenSceneMode.Single);

        Debug.Log("[BackpackLayoutSetup] 布局设置完成。请在 BackpackUI 上确认 itemSlotPrefab 已正确绑定。");
    }

    static void SetRect(RectTransform rt, float ax, float ay, float px, float py, float wx, float wy)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(ax + (wx <= 1 && wx > 0 ? wx : 0), ay + (wy <= 1 && wy > 0 ? wy : 0));
        rt.anchoredPosition = new Vector2(px, py);
        rt.sizeDelta = new Vector2(wx > 1 ? wx : 0, wy > 1 ? wy : 0);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
