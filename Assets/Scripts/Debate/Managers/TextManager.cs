using UnityEngine;
using System.IO;

namespace DebateSystem
{
    /// <summary>
    /// 文本/资源管理器。
    /// 负责从磁盘（StreamingAssets）读取并解析 JSON 剧本文件。
    /// </summary>
    public class TextManager : Singleton<TextManager>
    {
        #region 变量定义

        #endregion

        #region Unity 生命周期

        protected override void Awake()
        {
            base.Awake();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 加载指定名称的 JSON 剧本文件。
        /// </summary>
        /// <param name="filename">文件名（包含后缀，例如 "Chapter1.json"）</param>
        /// <returns>解析后的 DebateData 对象，如果失败返回 null</returns>
        public DebateData LoadScript(string filename)
        {
            string path = Path.Combine(Application.streamingAssetsPath, filename);
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                try 
                {
                    DebateData data = JsonUtility.FromJson<DebateData>(json);
                    return data;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"解析 JSON 失败 {filename}: {e.Message}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"未找到剧本文件: {path}");
                return null;
            }
        }

        #endregion
    }
}
