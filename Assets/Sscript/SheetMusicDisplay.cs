using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SheetMusicDisplay - 谱面滚动显示器
/// 
/// 功能：在屏幕上方滚动显示当前演奏的原始谱面字符串
/// 
/// 使用说明：
/// 1. 在music_scene场景中创建一个空的GameObject，命名为"SheetMusicDisplay"
/// 2. 将此脚本挂载到该GameObject上
/// 3. 创建UI显示组件：
///    a) 在Canvas下创建一个Panel作为背景（可选）：
///       - 设置锚点为顶部中心
///       - 调整大小覆盖屏幕顶部区域
///       - 设置半透明背景色
///    b) 在Panel下创建一个Text组件：
///       - 设置字体大小（建议24-32）
///       - 设置字体颜色（建议白色或亮色）
///       - 设置对齐方式为居中
///       - 将Text组件拖拽到本脚本的displayText字段
/// 4. 可选：创建一个Image作为当前位置指示器
///    - 将Image拖拽到本脚本的positionIndicator字段
/// 
/// 显示效果：
/// - 谱面字符串会在屏幕上方水平滚动
/// - 当前演奏位置会高亮显示（使用不同颜色）
/// - 已演奏部分和未演奏部分使用不同颜色区分
/// </summary>
public class SheetMusicDisplay : MonoBehaviour
{
    [Header("UI组件 - 需要在Inspector中绑定")]
    [Tooltip("用于显示谱面的Text组件")]
    public Text displayText;
    
    [Tooltip("当前位置指示器（可选）")]
    public Image positionIndicator;

    [Header("显示设置")]
    [Tooltip("显示的字符数量（当前位置前后各显示多少字符）")]
    public int displayRange = 30;
    
    [Tooltip("已演奏部分的颜色")]
    public Color playedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    [Tooltip("当前位置的颜色")]
    public Color currentColor = new Color(1f, 1f, 0f, 1f);
    
    [Tooltip("未演奏部分的颜色")]
    public Color unplayedColor = new Color(1f, 1f, 1f, 1f);

    [Header("滚动设置")]
    [Tooltip("是否启用平滑滚动")]
    public bool smoothScroll = true;
    
    [Tooltip("平滑滚动速度")]
    public float scrollSpeed = 5f;

    private int lastDisplayedIndex = -1;
    private float targetScrollPosition = 0f;
    private float currentScrollPosition = 0f;

    void Start()
    {
        if (displayText == null)
        {
            Debug.LogError("SheetMusicDisplay: displayText未设置！请在Inspector中绑定Text组件");
        }
    }

    void Update()
    {
        if (displayText == null) return;
        
        // 检查是否正在演奏
        if (!GameMenager.IsPlaying || string.IsNullOrEmpty(GameMenager.OriginalSheetContent))
        {
            displayText.text = "";
            return;
        }

        int currentIndex = GameMenager.CurrentSheetIndex;
        string sheetContent = GameMenager.OriginalSheetContent;

        // 只在位置变化时更新显示
        if (currentIndex != lastDisplayedIndex)
        {
            UpdateDisplay(sheetContent, currentIndex);
            lastDisplayedIndex = currentIndex;
        }
    }

    /// <summary>
    /// 更新谱面显示
    /// </summary>
    void UpdateDisplay(string sheetContent, int currentIndex)
    {
        // 计算显示范围
        int startIndex = Mathf.Max(0, currentIndex - displayRange);
        int endIndex = Mathf.Min(sheetContent.Length, currentIndex + displayRange);

        // 构建带颜色标记的显示字符串
        string displayString = "";

        // 已演奏部分（灰色）
        if (startIndex < currentIndex)
        {
            string playedPart = sheetContent.Substring(startIndex, currentIndex - startIndex);
            displayString += $"<color=#{ColorUtility.ToHtmlStringRGB(playedColor)}>{EscapeRichText(playedPart)}</color>";
        }

        // 当前位置（高亮）
        if (currentIndex < sheetContent.Length)
        {
            // 显示当前位置附近的几个字符作为高亮
            int highlightEnd = Mathf.Min(currentIndex + 3, sheetContent.Length);
            string currentPart = sheetContent.Substring(currentIndex, highlightEnd - currentIndex);
            displayString += $"<color=#{ColorUtility.ToHtmlStringRGB(currentColor)}><b>{EscapeRichText(currentPart)}</b></color>";
            
            // 未演奏部分（白色）
            if (highlightEnd < endIndex)
            {
                string unplayedPart = sheetContent.Substring(highlightEnd, endIndex - highlightEnd);
                displayString += $"<color=#{ColorUtility.ToHtmlStringRGB(unplayedColor)}>{EscapeRichText(unplayedPart)}</color>";
            }
        }

        displayText.text = displayString;
    }

    /// <summary>
    /// 转义Rich Text特殊字符
    /// </summary>
    string EscapeRichText(string text)
    {
        // 替换可能干扰Rich Text的字符
        return text.Replace("<", "&lt;").Replace(">", "&gt;");
    }

    /// <summary>
    /// 获取当前演奏进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        if (string.IsNullOrEmpty(GameMenager.OriginalSheetContent))
            return 0f;
        return (float)GameMenager.CurrentSheetIndex / GameMenager.OriginalSheetContent.Length;
    }

    /// <summary>
    /// 获取当前演奏位置的字符
    /// </summary>
    public char GetCurrentChar()
    {
        if (string.IsNullOrEmpty(GameMenager.OriginalSheetContent))
            return '\0';
        if (GameMenager.CurrentSheetIndex >= GameMenager.OriginalSheetContent.Length)
            return '\0';
        return GameMenager.OriginalSheetContent[GameMenager.CurrentSheetIndex];
    }
}
