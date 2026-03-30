using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using SFB;

public class SerialUIController : MonoBehaviour
{

 
    public TMP_InputField inputGap;
    public TMP_InputField inputScene;
    public TMP_InputField inputLoop;

    public TMP_Dropdown portDropdown;
    public TMP_Dropdown baudDropdown;
    public TMP_Text filePathText;

    
    private string selectedFilePath;

    void Start()
    {
        RefreshPorts();
        InitBaudRates();
    }

    // ===== 刷新串口 =====
    public void RefreshPorts()
    {
        portDropdown.ClearOptions();
    
        var ports = SerialManager.Instance.GetAvailablePorts();
    
        portDropdown.AddOptions(new System.Collections.Generic.List<string>(ports));
    }
    
    void InitBaudRates()
    {
        baudDropdown.ClearOptions();

        var baudList = new System.Collections.Generic.List<string>()
        {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"
        };

        baudDropdown.AddOptions(baudList);

        baudDropdown.value = 4; // 默认115200
    }
    

    // ===== 打开串口 =====
    public void OnOpenPort()
    {
        string port = portDropdown.options[portDropdown.value].text;

        int baud = int.Parse(baudDropdown.options[baudDropdown.value].text);

        SerialManager.Instance.SetPort(port, baud);
        SerialManager.Instance.OpenPort();
    }
    
    

    // ===== 选择 BIN 文件 =====
    public void OnSelectFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("选择BIN文件", "", "bin", false);

        if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            selectedFilePath = paths[0];

            // 显示完整路径
            // filePathText.text = selectedFilePath;
            //显示文件名
            filePathText.text = Path.GetFileName(selectedFilePath);

            Debug.Log("已选择文件：" + selectedFilePath);
        }
        else
        {
            Debug.Log("未选择文件");
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