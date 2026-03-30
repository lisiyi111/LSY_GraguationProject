// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using System;
//
// public class UILogger : MonoBehaviour
// {
//     public static UILogger Instance;
//
//     public TMP_Text logText;
//     public ScrollRect scrollRect;
//
//     private string content = "";
//
//     void Awake()
//     {
//         Instance = this;
//     }
//
//     public void Log(string msg)
//     {
//         string time = DateTime.Now.ToString("HH:mm:ss");
//
//         string line = $"[{time}] {msg}";
//
//         content += line + "\n";
//
//         if (content.Length > 8000)
//             content = content.Substring(content.Length - 8000);
//
//         logText.text = content;
//
//         // ⭐ 自动滚动到底部
//         Canvas.ForceUpdateCanvases();
//
//         scrollRect.content.GetComponent<RectTransform>().anchoredPosition =
//             new Vector2(0, scrollRect.content.sizeDelta.y);
//     }
// }

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

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
        string time = DateTime.Now.ToString("HH:mm:ss");
        string line = $"[{time}] {msg}";

        // ⭐ 判断当前是否在底部
        bool isAtBottom = scrollRect.verticalNormalizedPosition <= 0.01f;

        content += line + "\n";

        if (content.Length > 8000)
            content = content.Substring(content.Length - 8000);

        logText.text = content;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        Canvas.ForceUpdateCanvases();

        // ⭐ 只有在底部才自动滚
        if (isAtBottom)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    public void Clear()
    {
        content = "";
        logText.text = "";
    }
}