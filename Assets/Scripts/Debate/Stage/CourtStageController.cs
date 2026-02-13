using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DebateSystem
{
    /// <summary>
    /// 控制法庭全景卷轴动画的控制器。
    /// </summary>
    public class CourtStageController : MonoBehaviour
    {
        /// <summary>
        /// 运行时传递给法庭动画的角色数据结构
        /// </summary>
        public class CharacterRuntimeData
        {
            public string portraitPath; // 完整的立绘路径
            public bool useCustomPos;   // 是否使用自定义坐标
            public Vector2 customPos;   // 自定义坐标值
        }

        #region 变量定义

        [Header("Settings")]
        public float scrollSpeed = 200f; // 卷动速度 (像素/秒)
        public float initialTiltAngle = 5f; // 初始倾斜角度 (Z轴)
        public float characterScale = 0.5f; // 立绘缩放比例 (相对于 SetNativeSize)
        
        [Header("References")]
        public GameObject courtBGPrefab; // 用于克隆的模板 (CourtBG)
        
        private List<GameObject> spawnedBGs = new List<GameObject>();
        private bool isPlaying = false;
        private float totalWidth = 0f;
        private float currentScrollX = 0f;
        private float targetScrollX = 0f;
        private float initialX = 0f;
        
        private RectTransform rectTransform;
        private Quaternion targetRotation = Quaternion.identity;
        private Quaternion initialRotation;

        // 优化：使用字典缓存生成的对象，避免每次更新立绘都遍历列表
        private Dictionary<string, GameObject> spawnedBGMap = new Dictionary<string, GameObject>();

        #endregion

        #region Unity 生命周期

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (courtBGPrefab == null && transform.childCount > 0)
            {
                courtBGPrefab = transform.GetChild(0).gameObject;
            }
            
            if (courtBGPrefab != null)
            {
                courtBGPrefab.SetActive(false); // 隐藏模板
            }
        }

        private void Update()
        {
            if (!isPlaying) return;

            // 移动处理
            if (Mathf.Abs(currentScrollX - targetScrollX) > 1f)
            {
                // 向目标移动
                currentScrollX = Mathf.MoveTowards(currentScrollX, targetScrollX, scrollSpeed * Time.deltaTime);
                rectTransform.anchoredPosition = new Vector2(currentScrollX, 0);
                
                // 旋转插值处理
                // 计算进度 (0 ~ 1)
                float totalDistance = Mathf.Abs(targetScrollX - initialX);
                float currentDist = Mathf.Abs(currentScrollX - initialX);
                float progress = 0;
                if (totalDistance > 0)
                {
                    progress = currentDist / totalDistance;
                }
                
                // 随着移动，从 initialTiltAngle 变为 0
                transform.rotation = Quaternion.Lerp(initialRotation, Quaternion.identity, progress);
            }
            else
            {
                // 到达目标
                currentScrollX = targetScrollX;
                rectTransform.anchoredPosition = new Vector2(currentScrollX, 0);
                transform.rotation = Quaternion.identity;
                isPlaying = false;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化并开始卷轴动画
        /// </summary>
        /// <param name="charDataList">角色数据列表</param>
        public void Init(List<CharacterRuntimeData> charDataList)
        {
            if (courtBGPrefab == null)
            {
                Debug.LogError("[CourtStage] Template not found!");
                return;
            }

            Clear();

            gameObject.SetActive(true);
            initialRotation = Quaternion.Euler(0, 0, initialTiltAngle);
            transform.rotation = initialRotation;
            
            float currentX = 0;
            RectTransform templateRect = courtBGPrefab.GetComponent<RectTransform>();
            float itemWidth = templateRect.rect.width;

            for (int i = 0; i < charDataList.Count; i++)
            {
                CharacterRuntimeData data = charDataList[i];
                GameObject newBG = Instantiate(courtBGPrefab, transform);
                newBG.SetActive(true);
                // 使用路径中的名字部分作为对象名，方便调试和查找
                string bgName = "Unknown";
                if (!string.IsNullOrEmpty(data.portraitPath))
                {
                    string[] parts = data.portraitPath.Split('/');
                    if (parts.Length > 0) bgName = parts[0];
                }
                newBG.name = $"CourtBG_{bgName}";
                
                RectTransform bgRect = newBG.GetComponent<RectTransform>();
                
                itemWidth = bgRect.rect.width * bgRect.localScale.x; 
                
                bgRect.anchoredPosition = new Vector2(currentX, 0);
                
                SetupCharacter(newBG, data);
                
                spawnedBGs.Add(newBG);
                // 缓存到字典中，使用 bgName (从路径解析出的第一部分) 作为 Key
                if (!string.IsNullOrEmpty(bgName) && !spawnedBGMap.ContainsKey(bgName))
                {
                    spawnedBGMap.Add(bgName, newBG);
                }

                currentX += itemWidth; 
            }

            totalWidth = currentX;
            
            float screenWidth = 1920f; 
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas && canvas.GetComponent<RectTransform>())
            {
                screenWidth = canvas.GetComponent<RectTransform>().rect.width;
            }

            targetScrollX = screenWidth - totalWidth;
            
            if (targetScrollX > 0) targetScrollX = 0;

            initialX = rectTransform.anchoredPosition.x;
            currentScrollX = initialX;
            isPlaying = true;
        }

        /// <summary>
        /// 停止并隐藏
        /// </summary>
        public void StopAndHide()
        {
            isPlaying = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 动态更新指定角色的立绘
        /// </summary>
        /// <param name="portraitPath">立绘路径，例如 "Sherry/Sherry_Smile"</param>
        public void UpdateCharacterPortrait(string portraitPath)
        {
            if (string.IsNullOrEmpty(portraitPath) || portraitPath == "null") return;

            // 1. 从路径中提取角色名 (假设是第一部分)
            string[] parts = portraitPath.Split('/');
            if (parts.Length == 0) return;
            string charName = parts[0]; // e.g., "Sherry"

            // 2. 查找对应的对象
            // 使用字典快速查找
            if (spawnedBGMap.ContainsKey(charName))
            {
                SetupCharacter(spawnedBGMap[charName], portraitPath);
                return;
            }

            // 如果字典里没找到（可能是 Key 不匹配），尝试遍历查找（兼容旧逻辑）
            foreach (var bg in spawnedBGs)
            {
                if (bg.name.Contains(charName))
                {
                    SetupCharacter(bg, portraitPath);
                    // 同时更新字典，方便下次查找
                    if (!spawnedBGMap.ContainsKey(charName))
                    {
                        spawnedBGMap.Add(charName, bg);
                    }
                    return; 
                }
            }
        }

        #endregion

        #region 辅助方法

        private void SetupCharacter(GameObject bgObj, CharacterRuntimeData data)
        {
            if (bgObj.transform.childCount > 0)
            {
                // 假设第一个子对象是 CharacterShow
                Transform charTrans = bgObj.transform.GetChild(0);
                Image charImg = charTrans.GetComponent<Image>();
                RectTransform charRect = charTrans.GetComponent<RectTransform>();

                // 1. 设置立绘图片
                if (charImg)
                {
                    string fullPath = $"Image/Character/{data.portraitPath}";
                    Sprite sprite = Resources.Load<Sprite>(fullPath);

                    if (sprite != null)
                    {
                        charImg.sprite = sprite;
                        charImg.SetNativeSize();
                        // charImg.rectTransform.sizeDelta /= 2f; // Removed division by 2
                    }
                    else
                    {
                        Debug.LogWarning($"[CourtStage] Character sprite not found at: {fullPath}");
                    }
                }

                // 2. 设置自定义坐标 (如果有)
                if (data.useCustomPos && charRect != null)
                {
                    // 这里的坐标是相对于 CourtBG 的局部坐标
                    // 保持原有的 Z 轴或其他属性，仅修改 XY
                    Vector3 currentLoc = charRect.localPosition;
                    charRect.localPosition = new Vector3(data.customPos.x, data.customPos.y, currentLoc.z);
                }
            }
        }
        
        /// <summary>
        /// (Internal Helper) Overload for update method string path
        /// </summary>
        private void SetupCharacter(GameObject bgObj, string portraitPath)
        {
            CharacterRuntimeData dummyData = new CharacterRuntimeData 
            { 
                portraitPath = portraitPath, 
                useCustomPos = false 
            };
            SetupCharacter(bgObj, dummyData);
        }

        private void Clear()
        {
            foreach (var bg in spawnedBGs)
            {
                Destroy(bg);
            }
            spawnedBGs.Clear();
            spawnedBGMap.Clear();
            isPlaying = false;
        }

        #endregion
    }
}
