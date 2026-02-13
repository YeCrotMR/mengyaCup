using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError(this + "不符合单例模式");
        }
        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        Destroy();
    }

    /// <summary>
    /// 清除子类单例
    /// </summary>
    public void Destroy()
    {
        Instance = null;
    }
}

