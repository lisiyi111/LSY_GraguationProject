using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class UIDebug : MonoBehaviour
{
    public static UIDebug Instance;

    public TMP_Text logText;
    public ScrollRect scrollRect;

    private string content = "";

    void Awake()
    {
        Instance = this;

        // ⭐ 自动监听所有 Debug
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
            return;

        string time = DateTime.Now.ToString("HH:mm:ss");

        string color = "white";

        switch (type)
        {
            case LogType.Warning:
                color = "yellow";
                break;

            case LogType.Error:
            case LogType.Exception:
                color = "red";
                break;
        }

        string line = $"<color={color}>[{time}] {logString}</color>";

        AddLine(line);
    }

    // void AddLine(string line)
    // {
    //     content += line + "\n";
    //
    //     if (content.Length > 8000)
    //         content = content.Substring(content.Length - 8000);
    //
    //     logText.text = content;
    //
    //     LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    //     Canvas.ForceUpdateCanvases();
    //
    //     // ⭐ 延迟一帧滚动（最稳）
    //     StartCoroutine(ScrollToBottom());
    // }
    
    void AddLine(string line)
    {
        // ⭐ 只保留最新一条
        content = line;

        logText.text = content;

        // 强制刷新UI
        Canvas.ForceUpdateCanvases();

        // 永远在底部（其实已经不重要了）
        scrollRect.verticalNormalizedPosition = 0f;
    }

    IEnumerator ScrollToBottom()
    {
        yield return null; // 等一帧
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear()
    {
        content = "";
        logText.text = "";
    }
}