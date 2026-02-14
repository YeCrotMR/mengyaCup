using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包界面：右侧为信息种类标签（人物/疑点/结论），左侧/中间为列表与详情。
/// 点击主页面左上角背包按钮打开；出示证物时可自动弹出背包。
/// </summary>
public class BackpackUI : MonoBehaviour
{
    public static BackpackUI Instance { get; private set; }

    [Header("根面板")]
    public GameObject backpackPanel;
    public GameObject Black;

    [Header("右侧标签（信息种类）")]
    public Button tabCharacter;
    public Button tabSuspicion;
    public Button tabConclusion;
    public GameObject tabCharacterLockHint;
    public GameObject tabSuspicionLockHint;
    public GameObject tabConclusionLockHint;

    [Header("列表与详情")]
    public Transform itemListContent;
    public GameObject itemSlotPrefab;
    public GameObject detailPanel;
    public Text detailTitle;
    public Text detailDescription;
    public Image detailImage;
    [Tooltip("详情描述中的附加图片容器，若不设置则自动查找或创建")]
    public Transform detailImagesContent;
    public Button presentButton; // 新增：出示按钮

    [Header("详情描述图片设置")]
    [Tooltip("单张详情图片的显示尺寸")]
    public float detailImageSlotSize = 80f;

    private InfoCategory currentTab = InfoCategory.Character;
    private List<GameObject> slotInstances = new List<GameObject>();
    private List<GameObject> detailImageInstances = new List<GameObject>();
    
    // 选择模式相关
    private bool isSelectionMode = false;
    private System.Action<string> onEvidenceSelected;
    private BackpackItem currentSelectedItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (backpackPanel != null)
            backpackPanel.SetActive(false);
        

        if(Black != null)
            Black.SetActive(false);

        if (tabCharacter != null) tabCharacter.onClick.AddListener(() => SwitchTab(InfoCategory.Character));
        if (tabSuspicion != null) tabSuspicion.onClick.AddListener(() => SwitchTab(InfoCategory.Suspicion));
        if (tabConclusion != null) tabConclusion.onClick.AddListener(() => SwitchTab(InfoCategory.Conclusion));

        if (detailPanel != null)
            detailPanel.SetActive(false);
            
        if (presentButton != null)
        {
            presentButton.onClick.AddListener(OnPresentButtonClicked);
            presentButton.gameObject.SetActive(false);
        }
    }

    /// <summary>打开背包（可从背包按钮或出示证物时调用）</summary>
    public void OpenBackpack()
    {
        isSelectionMode = false;
        onEvidenceSelected = null;
        
        if (backpackPanel == null) return;

        backpackPanel.SetActive(true);
        Black.SetActive(true);
        currentTab = InfoCategory.Character; // 默认显示「人物」标签
        RefreshDisplay();
        
        // 默认选中并显示第一个人物（囚犯A）
        if (BackpackManager.Instance != null)
        {
            var chars = BackpackManager.Instance.GetItemsByCategory(InfoCategory.Character);
            if (chars.Count > 0)
                ShowDetail(chars[0]);
            else if (detailPanel != null)
                detailPanel.SetActive(false);
        }
        else if (detailPanel != null)
            detailPanel.SetActive(false);
    }
    
    /// <summary>
    /// 打开背包进入“出示证物”模式
    /// </summary>
    /// <param name="callback">当玩家点击出示按钮时触发的回调，参数为证物ID</param>
    public void OpenForSelection(System.Action<string> callback)
    {
        isSelectionMode = true;
        onEvidenceSelected = callback;
        currentSelectedItem = null; // 重置当前选择
        
        if (backpackPanel == null) return;

        backpackPanel.SetActive(true);
        Black.SetActive(true);
        currentTab = InfoCategory.Character; // 默认显示「人物」标签
        RefreshDisplay();
        
        // 默认选中并显示第一个人物（囚犯A），出示模式下不自动显示出示按钮直到用户点列表
        if (BackpackManager.Instance != null)
        {
            var chars = BackpackManager.Instance.GetItemsByCategory(InfoCategory.Character);
            if (chars.Count > 0)
                ShowDetail(chars[0]);
            else if (detailPanel != null)
                detailPanel.SetActive(false);
        }
        else if (detailPanel != null)
            detailPanel.SetActive(false);
        if (presentButton != null)
            presentButton.gameObject.SetActive(isSelectionMode && currentSelectedItem != null);
    }

    /// <summary>关闭背包</summary>
    public void CloseBackpack()
    {
        isSelectionMode = false;
        onEvidenceSelected = null;
        currentSelectedItem = null;
        
        if (backpackPanel == null) return;

        backpackPanel.SetActive(false);
        if(Black != null) Black.SetActive(false); // 修复：同时关闭遮罩

        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    /// <summary>出示证物时自动跳出背包</summary>
    public static void OpenForPresentingEvidence()
    {
        if (Instance != null)
            Instance.OpenBackpack();
    }

    public void RefreshDisplay()
    {
        UpdateTabLockHints();
        RefreshItemList();
        if (detailPanel != null && detailPanel.activeSelf && currentSelectedItem != null)
            ShowDetail(currentSelectedItem);
    }

    private void UpdateTabLockHints()
    {
        if (BackpackManager.Instance == null) return;

        if (tabCharacterLockHint != null)
            tabCharacterLockHint.SetActive(!BackpackManager.Instance.IsCategoryUnlocked(InfoCategory.Character));
        if (tabSuspicionLockHint != null)
            tabSuspicionLockHint.SetActive(!BackpackManager.Instance.IsCategoryUnlocked(InfoCategory.Suspicion));
        if (tabConclusionLockHint != null)
            tabConclusionLockHint.SetActive(!BackpackManager.Instance.IsCategoryUnlocked(InfoCategory.Conclusion));

        SetTabInteractable(tabCharacter, InfoCategory.Character);
        SetTabInteractable(tabSuspicion, InfoCategory.Suspicion);
        SetTabInteractable(tabConclusion, InfoCategory.Conclusion);
    }

    private void SetTabInteractable(Button btn, InfoCategory cat)
    {
        if (btn == null) return;
        btn.interactable = BackpackManager.Instance != null &&
                          BackpackManager.Instance.IsCategoryUnlocked(cat);
    }

    private void SwitchTab(InfoCategory category)
    {
        if (BackpackManager.Instance != null &&
            !BackpackManager.Instance.IsCategoryUnlocked(category))
            return;

        currentTab = category;
        RefreshItemList();

        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    private void RefreshItemList()
    {
        foreach (var go in slotInstances)
        {
            if (go != null) Destroy(go);
        }
        slotInstances.Clear();

        if (BackpackManager.Instance == null || itemListContent == null) return;
        if (!BackpackManager.Instance.IsCategoryUnlocked(currentTab)) return;

        List<BackpackItem> items = BackpackManager.Instance.GetItemsByCategory(currentTab);
        if (itemSlotPrefab == null)
        {
            Debug.LogWarning("[BackpackUI] 未设置 itemSlotPrefab，无法生成列表。");
            return;
        }

        bool useRuleLabel = (currentTab == InfoCategory.Conclusion);
        int index = 0;
        foreach (var item in items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemListContent);
            slotInstances.Add(slot);

            // 名称：结论标签用「规则一、规则二…」，其他用 title
            Text slotText = slot.GetComponentInChildren<Text>();
            if (slotText != null)
            {
                if (useRuleLabel)
                    slotText.text = GetConclusionSlotLabel(++index);
                else
                    slotText.text = item.title;
            }

            // 图标：结论标签下列表不显示图标
            Image slotImage = null;
            var iconTrans = slot.transform.Find("Icon");
            if (iconTrans != null)
                slotImage = iconTrans.GetComponent<Image>();

            if (slotImage != null)
            {
                if (useRuleLabel)
                {
                    slotImage.gameObject.SetActive(false);
                }
                else if (item.image != null)
                {
                    slotImage.sprite = item.image;
                    slotImage.preserveAspect = true;
                    slotImage.gameObject.SetActive(true);
                }
                else
                {
                    slotImage.gameObject.SetActive(false);
                }
            }

            // 点击显示详情
            Button slotBtn = slot.GetComponent<Button>();
            if (slotBtn != null)
            {
                var capture = item;
                slotBtn.onClick.AddListener(() => ShowDetail(capture));
            }
        }
    }

    private void ShowDetail(BackpackItem item)
    {
        currentSelectedItem = item;
        
        if (detailPanel != null) detailPanel.SetActive(true);
        if (detailTitle != null) detailTitle.text = item.title;
        if (detailDescription != null) detailDescription.text = item.description;

        bool isConclusion = item.category == InfoCategory.Conclusion;
        GameObject leftImageArea = GetDetailImageArea();
        if (leftImageArea != null)
            leftImageArea.SetActive(!isConclusion);

        if (detailImage != null && !isConclusion)
        {
            detailImage.gameObject.SetActive(item.image != null);
            detailImage.sprite = item.image;
        }

        // 结论不显示详情附加图，只显示文字
        if (isConclusion)
            RefreshDetailImages(null);
        else
            RefreshDetailImages(item.detailImages);

        // 如果是选择模式，显示出示按钮
        if (presentButton != null)
        {
            presentButton.gameObject.SetActive(isSelectionMode);
        }
    }

    /// <summary>结论列表项显示文字：规则一、规则二……</summary>
    private static string GetConclusionSlotLabel(int index)
    {
        string[] digits = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
        if (index >= 1 && index <= 10)
            return "规则" + digits[index - 1];
        return "规则" + index;
    }

    /// <summary>获取左侧大图区域（ItemImageArea），结论标签下隐藏</summary>
    private GameObject GetDetailImageArea()
    {
        if (detailImage != null && detailImage.transform.parent != null)
            return detailImage.transform.parent.gameObject;
        var detailArea = detailPanel != null ? detailPanel.transform.Find("ItemImageArea") : null;
        return detailArea != null ? detailArea.gameObject : null;
    }

    /// <summary>刷新详情描述区域中的附加图片</summary>
    private void RefreshDetailImages(Sprite[] images)
    {
        var container = GetOrCreateDetailImagesContent();
        if (container == null) return;

        // 清除已有实例
        foreach (var go in detailImageInstances)
        {
            if (go != null) Destroy(go);
        }
        detailImageInstances.Clear();

        if (images == null || images.Length == 0)
        {
            container.gameObject.SetActive(false);
            return;
        }

        container.gameObject.SetActive(true);
        float size = detailImageSlotSize > 0 ? detailImageSlotSize : 80f;

        foreach (var sprite in images)
        {
            if (sprite == null) continue;
            var imgGo = new GameObject("DetailImageSlot");
            imgGo.transform.SetParent(container, false);
            var imgRect = imgGo.AddComponent<RectTransform>();
            imgRect.sizeDelta = new Vector2(size, size);
            var img = imgGo.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.color = Color.white;
            detailImageInstances.Add(imgGo);
        }
    }

    /// <summary>获取或创建详情图片容器</summary>
    private Transform GetOrCreateDetailImagesContent()
    {
        if (detailImagesContent != null) return detailImagesContent;
        if (detailDescription == null) return null;

        var parent = detailDescription.transform.parent;
        if (parent == null) return null;

        var existing = parent.Find("DetailImagesContent");
        if (existing != null)
        {
            detailImagesContent = existing;
            return detailImagesContent;
        }

        var go = new GameObject("DetailImagesContent");
        go.transform.SetParent(parent, false);
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

        detailImagesContent = go.transform;
        return detailImagesContent;
    }
    
    private void OnPresentButtonClicked()
    {
        if (currentSelectedItem != null && onEvidenceSelected != null)
        {
            string id = currentSelectedItem.id;
            
            // 缓存回调，因为 CloseBackpack 会清空 onEvidenceSelected
            var callback = onEvidenceSelected;
            
            // 先关闭背包
            CloseBackpack();
            
            // 再触发回调
            callback.Invoke(id);
        }
    }
}
