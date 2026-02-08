using UnityEngine;

/// <summary>
/// 信息种类系统：背包中的信息分为 人物 / 疑点 / 结论 三种标签。
/// 人物一开始全部解锁（4人），疑点需搜寻后获得，结论在意识流后获得。
/// </summary>
public enum InfoCategory
{
    /// <summary>人物 - 初始解锁，每人可挂证词</summary>
    Character,

    /// <summary>疑点 - 玩家搜寻房间后获得</summary>
    Suspicion,

    /// <summary>结论 - 意识流阶段后获得</summary>
    Conclusion
}
