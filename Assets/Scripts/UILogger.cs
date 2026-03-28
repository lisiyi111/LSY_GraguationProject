using TMPro;
using UnityEngine;

public class UILogger : MonoBehaviour
{
    public static UILogger Instance;

    public TMP_Text logText;

    private string content = "";

    void Awake()
    {
        Instance = this;
    }

    public void Log(string msg)
    {
        content += msg + "\n";

        // 限制长度（防止卡顿）
        if (content.Length > 5000)
            content = content.Substring(content.Length - 5000);

        logText.text = content;
    }
}