using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class SerialUIController : MonoBehaviour
{

 
    public TMP_InputField inputGap;
    public TMP_InputField inputScene;
    public TMP_InputField inputLoop;

    public TMP_Dropdown portDropdown;
    public TMP_InputField baudInput;
    public TMP_Text filePathText;

    
    private string selectedFilePath;

    void Start()
    {
        RefreshPorts();
    }

    // ===== 刷新串口 =====
    public void RefreshPorts()
    {
        portDropdown.ClearOptions();
    
        var ports = SerialManager.Instance.GetAvailablePorts();
    
        portDropdown.AddOptions(new System.Collections.Generic.List<string>(ports));
    }
    

    // ===== 打开串口 =====
    public void OnOpenPort()
    {
        string port = portDropdown.options[portDropdown.value].text;
        int baud = int.Parse(baudInput.text);
    
        SerialManager.Instance.SetPort(port, baud);
        SerialManager.Instance.OpenPort();
    }
    
    

    // ===== 选择 BIN 文件 =====
    public void OnSelectFile()
    {
#if UNITY_EDITOR
        selectedFilePath = UnityEditor.EditorUtility.OpenFilePanel("选择BIN文件", "", "bin");
#endif

        if (!string.IsNullOrEmpty(selectedFilePath))
        {
            filePathText.text = selectedFilePath;
        }
    }

    // ===== 发送 BIN =====
    public void OnSendBin()
    {
        if (string.IsNullOrEmpty(selectedFilePath))
        {
            Debug.LogError("未选择文件！");
            return;
        }

        SerialManager.Instance.SendBinFile(selectedFilePath);
    }
    
    // ===== 按钮调用这个 =====
    
    public void OnSendRunCommand()
    {
        int gap = int.Parse(inputGap.text);
        int scene = int.Parse(inputScene.text);
        int loop = int.Parse(inputLoop.text);

        SerialManager.Instance.SendRunCommand(gap, scene, loop);
    }
}