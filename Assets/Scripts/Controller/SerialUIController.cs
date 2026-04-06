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
    public TMP_Text btnText;

    private bool isOpen;
    
    bool IsSerialOpen()
    {
        return SerialManager.Instance != null && SerialManager.Instance.IsOpen();
    }
    void Start()
    {
        RefreshPorts();
        InitBaudRates();
        isOpen = false;
        btnText.text = "打开串口";
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
    


    public void OnClickToggleSerial()
    {
        if (!isOpen)
        {
            //SerialManager.Instance.OpenPort();
            OnOpenPort();
            btnText.text = "关闭串口";
            isOpen = true;
        }
        else
        {
            SerialManager.Instance.ClosePort();
            btnText.text = "打开串口";
            isOpen = false;
        }
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

        if (SerialManager.Instance == null)
        {
            Debug.LogError("串口管理器不存在！");
            return;
        }

        // ⭐ 防止串口没开就发送
        if (!IsSerialOpen())
        {
            Debug.LogError("串口未打开！");
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
    
    public void OnSendClear()
    {
        SerialManager.Instance.SendClearCommand();
    }

    public void OnSendCheck()
    {
        SerialManager.Instance.SendCheckCommand();
    }
}