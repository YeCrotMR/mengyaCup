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
    public Button presentButton; // 新增：出示按钮

    private InfoCategory currentTab = InfoCategory.Character;
    private List<GameObject> slotInstances = new List<GameObject>();
    
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

        foreach (var item in items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemListContent);
            slotInstances.Add(slot);

            // 名称
            Text slotText = slot.GetComponentInChildren<Text>();
            if (slotText != null)
                slotText.text = item.title;

            // 图标
            Image slotImage = null;
            var iconTrans = slot.transform.Find("Icon");
            if (iconTrans != null)
                slotImage = iconTrans.GetComponent<Image>();

            if (slotImage != null)
            {
                if (item.image != null)
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

        if (detailImage != null)
        {
            detailImage.gameObject.SetActive(item.image != null);
            detailImage.sprite = item.image;
        }
        
        // 如果是选择模式，显示出示按钮
        if (presentButton != null)
        {
            presentButton.gameObject.SetActive(isSelectionMode);
        }
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
