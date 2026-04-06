using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class UILogger : MonoBehaviour
{
    public static UILogger Instance;

    public TMP_Text logText;
    public ScrollRect scrollRect;

    private string content = "";

    void Awake()
    {
        Instance = this;
    }
    

    public void Log(string msg)
    {
        // ⭐ 过滤不可显示字符（控制字符 + TMP 不支持字符）
        msg = Regex.Replace(msg, @"[\x00-\x1F\uFFFD]", "");

        if (string.IsNullOrEmpty(msg))
            return; // 过滤后为空就不显示

        string time = DateTime.Now.ToString("HH:mm:ss");
        string line = $"[{time}] {msg}";

        // 是否在底部
        bool isAtBottom = scrollRect.verticalNormalizedPosition <= 0.01f;

        content += line + "\n";

        if (content.Length > 8000)
            content = content.Substring(content.Length - 8000);

        logText.text = content;

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        Canvas.ForceUpdateCanvases();

        if (isAtBottom)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear()
    {
        content = "";
        logText.text = "";
    }
}