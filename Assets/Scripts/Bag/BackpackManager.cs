using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprite 数组的序列化包装，用于 Inspector 中配置人物详情图片等。
/// </summary>
[System.Serializable]
public class SpriteArray
{
    public Sprite[] sprites;
}

/// <summary>
/// 背包系统：管理证物、证词、人物等信息的存储与按标签筛选。
/// 关键信息与证词自动收录，出示证物时可自动打开背包。
/// </summary>
public class BackpackManager : MonoBehaviour
{
    public static BackpackManager Instance { get; private set; }

    [Header("初始人物（4人）")]
    [Tooltip("开局即解锁的人物 ID，用于背包「人物」标签")]
    public string[] initialCharacterIds = new string[] { "char_1", "char_2", "char_3", "char_4" };
    [Tooltip("对应显示名称，与上面 ID 一一对应")]
    public string[] initialCharacterNames = new string[] { "人物一", "人物二", "人物三", "人物四" };
    [Tooltip("对应人物头像（Sprite），与上面 ID 一一对应；可不设或数量不足则无图")]
    public Sprite[] initialCharacterSprites;
    [Tooltip("对应人物详情描述中的附加图片（每人的图片数组，数量不足则无图）")]
    public SpriteArray[] initialCharacterDetailImages;

    private List<BackpackItem> allItems = new List<BackpackItem>();
    private HashSet<string> addedIds = new HashSet<string>();
    private Dictionary<InfoCategory, bool> categoryUnlock = new Dictionary<InfoCategory, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 确保是根物体，否则 DontDestroyOnLoad 无效
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        categoryUnlock[InfoCategory.Character] = true;
        categoryUnlock[InfoCategory.Suspicion] = false;
        categoryUnlock[InfoCategory.Conclusion] = false;

        for (int i = 0; i < initialCharacterIds.Length; i++)
        {
            string cid = initialCharacterIds[i];
            if (string.IsNullOrEmpty(cid)) continue;
            if (addedIds.Contains(cid)) continue;
            addedIds.Add(cid);
            string name = (i < initialCharacterNames.Length && !string.IsNullOrEmpty(initialCharacterNames[i]))
                ? initialCharacterNames[i] : ("人物" + (i + 1));
            Sprite portrait = (initialCharacterSprites != null && i < initialCharacterSprites.Length)
                ? initialCharacterSprites[i] : null;
            Sprite[] detailImgs = (initialCharacterDetailImages != null && i < initialCharacterDetailImages.Length && initialCharacterDetailImages[i] != null)
                ? initialCharacterDetailImages[i].sprites : null;
            var item = new BackpackItem(cid, InfoCategory.Character, name, "（证词将记录在此）", portrait, cid);
            item.detailImages = detailImgs;
            allItems.Add(item);
        }
    }

    /// <summary>添加一条信息到背包（证物、证词等），自动去重</summary>
    public bool AddItem(BackpackItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.id))
        {
            Debug.LogWarning("[Backpack] 无效的 BackpackItem，未添加。");
            return false;
        }
        if (addedIds.Contains(item.id))
            return false;

        addedIds.Add(item.id);
        allItems.Add(item);

        if (item.category == InfoCategory.Suspicion)
            UnlockCategory(InfoCategory.Suspicion);

        if (BackpackUI.Instance != null)
            BackpackUI.Instance.RefreshDisplay();

        return true;
    }

    /// <summary>添加证物（关键线索），通常来自找寻阶段</summary>
    public bool AddEvidence(string id, string title, string description, Sprite image = null)
    {
        // 强制使用 title 作为 ID，确保 ID 与显示名称一致
        return AddItem(new BackpackItem(title, InfoCategory.Suspicion, title, description, image));
    }

    /// <summary>添加证词或重要文本，可关联到人物</summary>
    public bool AddTestimony(string id, string title, string description, string characterId = null, Sprite image = null)
    {
        var cat = string.IsNullOrEmpty(characterId) ? InfoCategory.Suspicion : InfoCategory.Character;
        // 强制使用 title 作为 ID
        return AddItem(new BackpackItem(title, cat, title, description, image, characterId));
    }

    /// <summary>添加结论（意识流后获得）</summary>
    public bool AddConclusion(string id, string title, string description, Sprite image = null)
    {
        // 强制使用 title 作为 ID
        return AddItem(new BackpackItem(title, InfoCategory.Conclusion, title, description, image));
    }

    /// <summary>解锁某类标签（疑点首次获得时解锁，结论在意识流后解锁）</summary>
    public void UnlockCategory(InfoCategory category)
    {
        categoryUnlock[category] = true;

        if (BackpackUI.Instance != null)
            BackpackUI.Instance.RefreshDisplay();
    }

    /// <summary>意识流后调用，解锁「结论」标签</summary>
    public void UnlockConclusionCategory()
    {
        UnlockCategory(InfoCategory.Conclusion);
    }

    public bool IsCategoryUnlocked(InfoCategory category)
    {
        return categoryUnlock.ContainsKey(category) && categoryUnlock[category];
    }

    /// <summary>按标签获取当前已添加的条目（人物类按 characterId 聚合显示由 UI 处理）</summary>
    public List<BackpackItem> GetItemsByCategory(InfoCategory category)
    {
        var list = new List<BackpackItem>();
        foreach (var item in allItems)
        {
            if (item.category == category)
                list.Add(item);
        }
        return list;
    }

    public List<BackpackItem> GetAllItems() => new List<BackpackItem>(allItems);

    /// <summary>是否已包含某 id</summary>
    public bool HasItem(string id) => addedIds.Contains(id);

    /// <summary>更新指定条目的详情描述图片（如人物详情中的附加图片）</summary>
    public void UpdateItemDetailImages(string id, Sprite[] detailImages)
    {
        foreach (var item in allItems)
        {
            if (item.id == id)
            {
                item.detailImages = detailImages;
                if (BackpackUI.Instance != null)
                    BackpackUI.Instance.RefreshDisplay();
                return;
            }
        }
    }
}
