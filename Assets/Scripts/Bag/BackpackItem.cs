using UnityEngine;

/// <summary>
/// 背包中的单条信息：证物、证词、人物信息等。
/// </summary>
[System.Serializable]
public class BackpackItem
{
    /// <summary>唯一 ID，用于去重与查找</summary>
    public string id;

    /// <summary>所属标签：人物 / 疑点 / 结论</summary>
    public InfoCategory category;

    /// <summary>显示标题（如证物名、人物名、疑点简述）</summary>
    public string title;

    /// <summary>详细描述或证词文本</summary>
    [TextArea(2, 6)]
    public string description;

    /// <summary>特写图片（证物、关键信息图）</summary>
    public Sprite image;

    /// <summary>详情描述中的附加图片（人物等可配置多张图，显示在描述区域下方）</summary>
    public Sprite[] detailImages;

    /// <summary>若为人物类，可关联该人物下的证词列表（由 BackpackManager 按人物聚合）</summary>
    public string characterId;

    public BackpackItem() { }

    public BackpackItem(string id, InfoCategory category, string title, string description, Sprite image = null, string characterId = null)
    {
        this.id = id;
        this.category = category;
        this.title = title;
        this.description = description;
        this.image = image;
        this.characterId = characterId;
    }
}
