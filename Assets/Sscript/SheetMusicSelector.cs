using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// SheetMusicSelector - 谱面选择器
/// 
/// 使用说明：
/// 1. 在front_page场景中创建一个空的GameObject，命名为"SheetMusicSelector"
/// 2. 将此脚本挂载到该GameObject上
/// 3. 在场景中创建一个Canvas（如果没有的话）：
///    - GameObject -> UI -> Canvas
///    - 设置Canvas的Render Mode为"Screen Space - Overlay"
/// 4. 在Canvas下创建一个ScrollView用于显示谱面列表：
///    - GameObject -> UI -> Scroll View
///    - 将ScrollView的Content对象拖拽到本脚本的contentParent字段
/// 5. 创建一个按钮预制件（Prefab）：
///    - 在Canvas下创建一个Button：GameObject -> UI -> Button
///    - 调整按钮大小（建议宽度400，高度60）
///    - 将按钮拖拽到Assets/prefab文件夹创建预制件
///    - 将预制件拖拽到本脚本的buttonPrefab字段
///    - 删除场景中的临时按钮
/// 6. 可选：创建一个Text显示当前选择状态
///    - 将Text对象拖拽到本脚本的statusText字段
/// 
/// 目录结构要求：
/// - StreamingAssets/mid/     - 存放MIDI文件（.mid）
/// - StreamingAssets/gua/     - 存放呱呱谱面文件（.txt）
/// - StreamingAssets/SheetMusic/ - 游戏读取谱面的目录
/// </summary>
public class SheetMusicSelector : MonoBehaviour
{
    [Header("UI组件 - 需要在Inspector中绑定")]
    [Tooltip("ScrollView的Content对象，用于放置按钮列表")]
    public Transform contentParent;
    
    [Tooltip("按钮预制件，用于生成谱面选择按钮")]
    public GameObject buttonPrefab;
    
    [Tooltip("状态文本，显示当前选择的谱面（可选）")]
    public Text statusText;
    
    [Header("UI布局设置")]
    [Tooltip("按钮之间的垂直间距")]
    public float buttonSpacing = 10f;
    
    [Tooltip("按钮高度")]
    public float buttonHeight = 60f;
    
    [Header("颜色设置")]
    [Tooltip("MIDI谱面按钮颜色")]
    public Color midiButtonColor = new Color(0.3f, 0.6f, 1f, 1f);
    
    [Tooltip("呱呱谱面按钮颜色")]
    public Color guaButtonColor = new Color(0.3f, 1f, 0.6f, 1f);

    // 谱面信息类
    public class SheetInfo
    {
        public string name;           // 显示名称
        public string fullPath;       // 完整路径
        public SheetType type;        // 谱面类型
        public string yamlPath;       // 对应的yaml配置文件路径（如果有）
    }

    public enum SheetType
    {
        MIDI,   // MIDI文件，需要自动转换
        GUA     // 呱呱谱面，直接复制内容
    }

    private List<SheetInfo> sheetList = new List<SheetInfo>();
    private string midPath;
    private string guaPath;
    private string sheetMusicPath;

    void Start()
    {
        // 初始化路径
        midPath = Path.Combine(Application.streamingAssetsPath, "mid");
        guaPath = Path.Combine(Application.streamingAssetsPath, "gua");
        sheetMusicPath = Path.Combine(Application.streamingAssetsPath, "SheetMusic");

        // 确保目录存在
        EnsureDirectoriesExist();
        
        // 扫描谱面文件
        ScanSheetMusic();
        
        // 生成UI
        GenerateUI();
        
        UpdateStatus("请选择一个谱面");
    }

    /// <summary>
    /// 确保必要的目录存在
    /// </summary>
    void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(midPath))
        {
            Directory.CreateDirectory(midPath);
            Debug.Log($"创建目录: {midPath}");
        }
        if (!Directory.Exists(guaPath))
        {
            Directory.CreateDirectory(guaPath);
            Debug.Log($"创建目录: {guaPath}");
        }
        if (!Directory.Exists(sheetMusicPath))
        {
            Directory.CreateDirectory(sheetMusicPath);
            Debug.Log($"创建目录: {sheetMusicPath}");
        }
    }

    /// <summary>
    /// 扫描mid和gua目录下的谱面文件
    /// </summary>
    void ScanSheetMusic()
    {
        sheetList.Clear();

        // 扫描MIDI文件
        if (Directory.Exists(midPath))
        {
            string[] midFiles = Directory.GetFiles(midPath, "*.mid");
            foreach (string file in midFiles)
            {
                SheetInfo info = new SheetInfo
                {
                    name = "[MIDI] " + Path.GetFileNameWithoutExtension(file),
                    fullPath = file,
                    type = SheetType.MIDI
                };
                sheetList.Add(info);
                Debug.Log($"发现MIDI谱面: {info.name}");
            }
        }

        // 扫描呱呱谱面文件
        if (Directory.Exists(guaPath))
        {
            string[] guaFiles = Directory.GetFiles(guaPath, "*.txt");
            foreach (string file in guaFiles)
            {
                string baseName = Path.GetFileNameWithoutExtension(file);
                string yamlPath = Path.Combine(guaPath, baseName + ".yaml");
                
                SheetInfo info = new SheetInfo
                {
                    name = "[呱呱] " + baseName,
                    fullPath = file,
                    type = SheetType.GUA,
                    yamlPath = File.Exists(yamlPath) ? yamlPath : null
                };
                sheetList.Add(info);
                Debug.Log($"发现呱呱谱面: {info.name}" + (info.yamlPath != null ? " (有配置文件)" : " (无配置文件)"));
            }
        }

        Debug.Log($"共发现 {sheetList.Count} 个谱面");
    }

    /// <summary>
    /// 动态生成谱面选择UI
    /// </summary>
    void GenerateUI()
    {
        if (contentParent == null)
        {
            Debug.LogError("contentParent未设置！请在Inspector中绑定ScrollView的Content对象");
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("buttonPrefab未设置！请在Inspector中绑定按钮预制件");
            return;
        }

        // 清除现有按钮
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 设置Content的大小
        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = sheetList.Count * (buttonHeight + buttonSpacing);
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
        }

        // 为每个谱面创建按钮
        for (int i = 0; i < sheetList.Count; i++)
        {
            SheetInfo info = sheetList[i];
            
            // 实例化按钮
            GameObject buttonObj = Instantiate(buttonPrefab, contentParent);
            
            // 设置按钮位置
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0, 1);
                buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.pivot = new Vector2(0.5f, 1);
                buttonRect.anchoredPosition = new Vector2(0, -i * (buttonHeight + buttonSpacing));
                buttonRect.sizeDelta = new Vector2(-20, buttonHeight);
            }

            // 设置按钮文本
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = info.name;
            }
            
            // 设置按钮颜色
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = info.type == SheetType.MIDI ? midiButtonColor : guaButtonColor;
            }

            // 添加点击事件
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                SheetInfo capturedInfo = info; // 捕获变量
                button.onClick.AddListener(() => OnSheetSelected(capturedInfo));
            }
        }

        // 如果没有谱面，显示提示
        if (sheetList.Count == 0)
        {
            UpdateStatus("未找到谱面文件\n请将MIDI文件放入StreamingAssets/mid/\n或将呱呱谱面放入StreamingAssets/gua/");
        }
    }

    /// <summary>
    /// 谱面选择回调
    /// </summary>
    void OnSheetSelected(SheetInfo info)
    {
        Debug.Log($"选择谱面: {info.name}");
        UpdateStatus($"正在加载: {info.name}");

        try
        {
            if (info.type == SheetType.MIDI)
            {
                LoadMidiSheet(info);
            }
            else
            {
                LoadGuaSheet(info);
            }

            UpdateStatus($"已加载: {info.name}\n即将开始游戏...");
            
            // 延迟加载游戏场景
            StartCoroutine(LoadGameSceneDelayed(1.5f));
        }
        catch (Exception e)
        {
            Debug.LogError($"加载谱面失败: {e.Message}");
            UpdateStatus($"加载失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载MIDI谱面
    /// 将MIDI文件复制到SheetMusic目录，程序会自动读取并转换
    /// </summary>
    void LoadMidiSheet(SheetInfo info)
    {
        // 清除SheetMusic目录下的旧MIDI文件
        string[] existingMidFiles = Directory.GetFiles(sheetMusicPath, "*.mid");
        foreach (string file in existingMidFiles)
        {
            File.Delete(file);
            Debug.Log($"删除旧MIDI文件: {file}");
        }

        // 复制新的MIDI文件到SheetMusic目录
        string destPath = Path.Combine(sheetMusicPath, Path.GetFileName(info.fullPath));
        File.Copy(info.fullPath, destPath, true);
        Debug.Log($"复制MIDI文件: {info.fullPath} -> {destPath}");
    }

    /// <summary>
    /// 加载呱呱谱面
    /// 将谱面内容复制到test_sheet_music.txt
    /// 如果有对应的yaml配置文件，也一并复制
    /// </summary>
    void LoadGuaSheet(SheetInfo info)
    {
        // 清除SheetMusic目录下的MIDI文件（避免MIDI优先加载）
        string[] existingMidFiles = Directory.GetFiles(sheetMusicPath, "*.mid");
        foreach (string file in existingMidFiles)
        {
            File.Delete(file);
            Debug.Log($"删除MIDI文件: {file}");
        }

        // 读取谱面内容
        string sheetContent = File.ReadAllText(info.fullPath);
        
        // 写入到test_sheet_music.txt
        string destSheetPath = Path.Combine(sheetMusicPath, "test_sheet_music.txt");
        File.WriteAllText(destSheetPath, sheetContent);
        Debug.Log($"写入谱面内容到: {destSheetPath}");

        // 如果有对应的yaml配置文件，也复制过去
        if (!string.IsNullOrEmpty(info.yamlPath) && File.Exists(info.yamlPath))
        {
            string yamlContent = File.ReadAllText(info.yamlPath);
            string destYamlPath = Path.Combine(sheetMusicPath, "test_sheet_music.yaml");
            File.WriteAllText(destYamlPath, yamlContent);
            Debug.Log($"复制配置文件: {info.yamlPath} -> {destYamlPath}");
        }
        else
        {
            Debug.LogWarning($"谱面 {info.name} 没有对应的yaml配置文件，请手动修改test_sheet_music.yaml");
        }
    }

    /// <summary>
    /// 延迟加载游戏场景
    /// </summary>
    IEnumerator LoadGameSceneDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("music_scene");
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"状态: {message}");
    }

    /// <summary>
    /// 刷新谱面列表（可以绑定到刷新按钮）
    /// </summary>
    public void RefreshSheetList()
    {
        ScanSheetMusic();
        GenerateUI();
        UpdateStatus("谱面列表已刷新");
    }
}
