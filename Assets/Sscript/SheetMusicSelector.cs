using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // 支持 TextMeshPro

/// <summary>
/// SheetMusicSelector - 谱面选择器（弹窗版）
/// 
/// 使用说明：
/// 1. 在front_page场景中创建一个空的GameObject，命名为"SheetMusicSelector"
/// 2. 将此脚本挂载到该GameObject上
/// 3. 创建弹窗面板：
///    - 在Canvas下创建一个Panel：GameObject -> UI -> Panel
///    - 命名为"SheetMusicPopup"
///    - 调整大小和位置（建议居中，适当大小）
///    - 将Panel拖拽到本脚本的popupPanel字段
/// 4. 在弹窗Panel内创建ScrollView：
///    - 右键Panel -> UI -> Scroll View
///    - 将ScrollView的Content对象拖拽到本脚本的contentParent字段
/// 5. 创建关闭按钮（可选）：
///    - 在弹窗Panel内创建Button，文字设为"X"或"关闭"
///    - 将按钮拖拽到本脚本的closeButton字段
/// 6. 创建谱面按钮预制件：
///    - 创建一个Button，调整大小（建议宽度400，高度60）
///    - 拖拽到Assets/prefab创建预制件
///    - 将预制件拖拽到本脚本的buttonPrefab字段
/// 7. 创建"选择谱面"按钮：
///    - 在Canvas下创建一个按钮
///    - 在按钮的OnClick事件中，绑定SheetMusicSelector的OpenPopup方法
/// 
/// 目录结构要求：
/// - StreamingAssets/mid/     - 存放MIDI文件（.mid）
/// - StreamingAssets/gua/     - 存放呱呱谱面文件（.txt）
/// - StreamingAssets/SheetMusic/ - 游戏读取谱面的目录
/// </summary>
public class SheetMusicSelector : MonoBehaviour
{
    [Header("UI组件 - 需要在Inspector中绑定")]
    [Tooltip("弹窗面板，包含整个选择界面")]
    public GameObject popupPanel;
    
    [Tooltip("ScrollView的Content对象，用于放置按钮列表")]
    public Transform contentParent;
    
    [Tooltip("按钮预制件，用于生成谱面选择按钮")]
    public GameObject buttonPrefab;
    
    [Tooltip("状态文本，显示当前选择的谱面（可选）")]
    public Text statusText;
    
    [Tooltip("关闭按钮（可选）")]
    public Button closeButton;
    
    [Tooltip("选择谱面按钮（打开弹窗时隐藏）")]
    public GameObject selectSheetButton;
    
    [Header("标签切换按钮")]
    [Tooltip("MIDI标签按钮")]
    public Button midiTabButton;
    
    [Tooltip("呱呱标签按钮")]
    public Button guaTabButton;
    
    [Header("标签按钮颜色")]
    [Tooltip("标签选中时的颜色")]
    public Color tabSelectedColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("标签未选中时的颜色")]
    public Color tabNormalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    
    [Header("Base Interval 编辑（呱呱谱面用）")]
    [Tooltip("Base Interval 编辑面板")]
    public GameObject baseIntervalPanel;
    
    [Tooltip("Base Interval 输入框（普通InputField）")]
    public InputField baseIntervalInput;
    
    [Tooltip("Base Interval 输入框（TMP版本，二选一）")]
    public TMP_InputField baseIntervalInputTMP;
    
    [Tooltip("Base Interval 标签文本")]
    public Text baseIntervalLabel;
    
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
    
    // 当前显示的谱面类型（null表示显示全部）
    private SheetType? currentFilter = null;

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
        
        // 默认隐藏弹窗和相关UI
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        if (baseIntervalPanel != null)
        {
            baseIntervalPanel.SetActive(false);
        }
        
        // 绑定关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        
        // 绑定标签按钮事件
        if (midiTabButton != null)
        {
            midiTabButton.onClick.AddListener(() => SwitchTab(SheetType.MIDI));
        }
        if (guaTabButton != null)
        {
            guaTabButton.onClick.AddListener(() => SwitchTab(SheetType.GUA));
        }
        
        // 绑定 base_interval 输入框事件
        if (baseIntervalInput != null)
        {
            baseIntervalInput.onEndEdit.AddListener(OnBaseIntervalChanged);
        }
        if (baseIntervalInputTMP != null)
        {
            baseIntervalInputTMP.onEndEdit.AddListener(OnBaseIntervalChanged);
        }
        
        UpdateStatus("请选择一个谱面");
    }
    
    /// <summary>
    /// 切换标签页
    /// </summary>
    public void SwitchTab(SheetType type)
    {
        currentFilter = type;
        GenerateUI();
        UpdateTabButtonColors();
        
        string typeName = type == SheetType.MIDI ? "MIDI" : "呱呱";
        UpdateStatus($"显示 {typeName} 谱面");
    }
    
    /// <summary>
    /// 显示全部谱面
    /// </summary>
    public void ShowAllSheets()
    {
        currentFilter = null;
        GenerateUI();
        UpdateTabButtonColors();
        UpdateStatus("显示全部谱面");
    }
    
    /// <summary>
    /// 更新标签按钮的颜色
    /// </summary>
    void UpdateTabButtonColors()
    {
        if (midiTabButton != null)
        {
            var colors = midiTabButton.colors;
            colors.normalColor = (currentFilter == SheetType.MIDI) ? tabSelectedColor : tabNormalColor;
            midiTabButton.colors = colors;
        }
        
        if (guaTabButton != null)
        {
            var colors = guaTabButton.colors;
            colors.normalColor = (currentFilter == SheetType.GUA) ? tabSelectedColor : tabNormalColor;
            guaTabButton.colors = colors;
        }
    }
    
    /// <summary>
    /// 打开谱面选择弹窗（绑定到"选择谱面"按钮）
    /// </summary>
    public void OpenPopup()
    {
        // 每次打开时刷新列表
        ScanSheetMusic();
        
        // 默认显示 MIDI 谱面
        currentFilter = SheetType.MIDI;
        GenerateUI();
        UpdateTabButtonColors();
        
        // 加载 base_interval 值
        LoadBaseInterval();
        
        // 隐藏"选择谱面"按钮
        if (selectSheetButton != null)
        {
            selectSheetButton.SetActive(false);
        }
        
        // 显示弹窗和 base_interval 面板
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
        if (baseIntervalPanel != null)
        {
            baseIntervalPanel.SetActive(true);
        }
        UpdateStatus("显示 MIDI 谱面");
    }
    
    /// <summary>
    /// 关闭谱面选择弹窗
    /// </summary>
    public void ClosePopup()
    {
        // 隐藏弹窗和 base_interval 面板
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        if (baseIntervalPanel != null)
        {
            baseIntervalPanel.SetActive(false);
        }
        
        // 显示"选择谱面"按钮
        if (selectSheetButton != null)
        {
            selectSheetButton.SetActive(true);
        }
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

        // 扫描MIDI文件（只扫描当前目录，排除子目录和.meta文件）
        if (Directory.Exists(midPath))
        {
            string[] midFiles = Directory.GetFiles(midPath, "*.mid", SearchOption.TopDirectoryOnly);
            foreach (string file in midFiles)
            {
                // 跳过.meta文件和空文件名
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrEmpty(fileName) || file.EndsWith(".meta"))
                    continue;
                    
                SheetInfo info = new SheetInfo
                {
                    name = "[MIDI] " + fileName,
                    fullPath = file,
                    type = SheetType.MIDI
                };
                sheetList.Add(info);
                Debug.Log($"发现MIDI谱面: {info.name}");
            }
        }

        // 扫描呱呱谱面文件（只扫描当前目录，排除子目录和.meta文件）
        if (Directory.Exists(guaPath))
        {
            string[] guaFiles = Directory.GetFiles(guaPath, "*.txt", SearchOption.TopDirectoryOnly);
            foreach (string file in guaFiles)
            {
                // 跳过.meta文件和空文件名
                string baseName = Path.GetFileNameWithoutExtension(file);
                if (string.IsNullOrEmpty(baseName) || file.EndsWith(".meta"))
                    continue;
                    
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

        // 根据过滤条件获取要显示的谱面列表
        List<SheetInfo> filteredList = new List<SheetInfo>();
        foreach (var sheet in sheetList)
        {
            if (currentFilter == null || sheet.type == currentFilter)
            {
                filteredList.Add(sheet);
            }
        }

        // 设置Content的大小
        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = filteredList.Count * (buttonHeight + buttonSpacing);
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
        }

        // 为每个谱面创建按钮
        for (int i = 0; i < filteredList.Count; i++)
        {
            SheetInfo info = filteredList[i];
            
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

            // 设置按钮文本（同时支持普通Text和TextMeshPro）
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            TMP_Text tmpText = buttonObj.GetComponentInChildren<TMP_Text>();
            
            if (buttonText != null)
            {
                buttonText.text = info.name;
                // 修复长文件名显示问题
                buttonText.horizontalOverflow = HorizontalWrapMode.Overflow;
                buttonText.verticalOverflow = VerticalWrapMode.Overflow;
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 10;
                buttonText.resizeTextMaxSize = 24;
                buttonText.alignment = TextAnchor.MiddleLeft;
                
                // 强制设置 RectTransform 为左对齐
                RectTransform textRect = buttonText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = new Vector2(0, 0);
                    textRect.anchorMax = new Vector2(1, 1);
                    textRect.offsetMin = new Vector2(10, 0);  // 左边距 10
                    textRect.offsetMax = new Vector2(-10, 0); // 右边距 10
                }
            }
            else if (tmpText != null)
            {
                tmpText.text = info.name;
                // 修复长文件名显示问题
                tmpText.overflowMode = TextOverflowModes.Ellipsis;
                tmpText.enableAutoSizing = true;
                tmpText.fontSizeMin = 10;
                tmpText.fontSizeMax = 24;
                tmpText.alignment = TextAlignmentOptions.Left;
                
                // 强制设置 RectTransform 为左对齐
                RectTransform textRect = tmpText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = new Vector2(0, 0);
                    textRect.anchorMax = new Vector2(1, 1);
                    textRect.offsetMin = new Vector2(10, 0);  // 左边距 10
                    textRect.offsetMax = new Vector2(-10, 0); // 右边距 10
                }
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
        if (filteredList.Count == 0)
        {
            if (currentFilter == SheetType.MIDI)
            {
                UpdateStatus("未找到MIDI谱面\n请将MIDI文件放入StreamingAssets/mid/");
            }
            else if (currentFilter == SheetType.GUA)
            {
                UpdateStatus("未找到呱呱谱面\n请将呱呱谱面放入StreamingAssets/gua/");
            }
            else
            {
                UpdateStatus("未找到谱面文件\n请将MIDI文件放入StreamingAssets/mid/\n或将呱呱谱面放入StreamingAssets/gua/");
            }
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
            
            // 关闭弹窗
            ClosePopup();
            
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
    
    #region Base Interval 编辑功能
    
    /// <summary>
    /// 从 yaml 文件加载 base_interval 值
    /// </summary>
    void LoadBaseInterval()
    {
        string yamlPath = Path.Combine(sheetMusicPath, "test_sheet_music.yaml");
        
        if (File.Exists(yamlPath))
        {
            try
            {
                string[] lines = File.ReadAllLines(yamlPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("base_interval:"))
                    {
                        string value = line.Substring("base_interval:".Length).Trim();
                        SetBaseIntervalInputValue(value);
                        Debug.Log($"读取 base_interval: {value}");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取 yaml 文件失败: {e.Message}");
            }
        }
        
        // 默认值
        SetBaseIntervalInputValue("125");
    }
    
    /// <summary>
    /// 设置输入框的值
    /// </summary>
    void SetBaseIntervalInputValue(string value)
    {
        if (baseIntervalInput != null)
        {
            baseIntervalInput.text = value;
        }
        if (baseIntervalInputTMP != null)
        {
            baseIntervalInputTMP.text = value;
        }
    }
    
    /// <summary>
    /// 获取输入框的值
    /// </summary>
    string GetBaseIntervalInputValue()
    {
        if (baseIntervalInput != null)
        {
            return baseIntervalInput.text;
        }
        if (baseIntervalInputTMP != null)
        {
            return baseIntervalInputTMP.text;
        }
        return "125";
    }
    
    /// <summary>
    /// 当 base_interval 输入框值改变时调用
    /// </summary>
    void OnBaseIntervalChanged(string newValue)
    {
        // 验证输入是否为有效数字
        if (int.TryParse(newValue, out int interval))
        {
            SaveBaseInterval(interval);
            UpdateStatus($"base_interval 已更新为: {interval}");
        }
        else
        {
            UpdateStatus("请输入有效的数字");
            LoadBaseInterval(); // 恢复原值
        }
    }
    
    /// <summary>
    /// 保存 base_interval 到 yaml 文件
    /// </summary>
    void SaveBaseInterval(int value)
    {
        string yamlPath = Path.Combine(sheetMusicPath, "test_sheet_music.yaml");
        
        try
        {
            if (File.Exists(yamlPath))
            {
                string[] lines = File.ReadAllLines(yamlPath);
                bool found = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("base_interval:"))
                    {
                        lines[i] = $"base_interval: {value}";
                        found = true;
                        break;
                    }
                }
                
                if (found)
                {
                    File.WriteAllLines(yamlPath, lines);
                    Debug.Log($"保存 base_interval: {value}");
                }
                else
                {
                    // 如果没找到，在文件开头添加
                    var newLines = new List<string> { $"base_interval: {value}" };
                    newLines.AddRange(lines);
                    File.WriteAllLines(yamlPath, newLines);
                    Debug.Log($"添加 base_interval: {value}");
                }
            }
            else
            {
                // 创建新文件
                File.WriteAllText(yamlPath, $"base_interval: {value}\n");
                Debug.Log($"创建 yaml 并设置 base_interval: {value}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"保存 yaml 文件失败: {e.Message}");
        }
    }
    
    #endregion
}
