using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialManager : MonoBehaviour
{
    public static SerialManager Instance;

    public string portName = "COM3";
    public int baudRate = 115200;

    private SerialPort serialPort;
    private Thread receiveThread;
    private bool isRunning = false;

    void Awake()
    {
        Instance = this;
    }
    
    public void SetPort(string port, int baud)
    {
        portName = port;
        baudRate = baud;
    }
    
    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    // ===== 打开串口 =====
    public void OpenPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();

            isRunning = true;
            receiveThread = new Thread(ReadData);
            receiveThread.Start();

            Debug.Log("串口打开成功");
            UILogger.Instance?.Log($"Serial port opened successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("串口打开失败：" + e.Message);
            UILogger.Instance?.Log($"Serial port opening failed:" + e.Message);
        }
    }

    // ===== 关闭串口 =====
    public void ClosePort()
    {
        isRunning = false;

        if (receiveThread != null)
            receiveThread.Abort();

        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();

        Debug.Log("串口已关闭");
        UILogger.Instance?.Log($"The serial port is closed");
    }

    // ===== 发送数据 =====
    public void SendBytes(byte[] data)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(data, 0, data.Length);
            string msg = " TX: " + BitConverter.ToString(data);
            UILogger.Instance?.Log(msg);
            Debug.Log(msg);
        }
    }

    // ===== 接收线程 =====
    void ReadData()
    {
        while (isRunning)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string msg = serialPort.ReadLine();
                    Debug.Log("接收：" + msg);
                    UILogger.Instance?.Log(" RX: " + msg);

                    ParseMessage(msg);
                }
            }
            catch
            {
            }
        }
    }

    public void SendRunCommand(int gap, int sceneCount, int loop)
    {
        byte[] data = new byte[10];

        data[0] = 0xF8;

        // gap（1字节 → 2字节）
        data[1] = (byte)(gap / 10);
        data[2] = (byte)(gap % 10);

        // sceneCount（2字节 → 4字节）
        data[3] = (byte)((sceneCount >> 12) & 0x0F);
        data[4] = (byte)((sceneCount >> 8) & 0x0F);
        data[5] = (byte)((sceneCount >> 4) & 0x0F);
        data[6] = (byte)(sceneCount & 0x0F);

        // loop（1字节 → 2字节）
        data[7] = (byte)(loop / 10);
        data[8] = (byte)(loop % 10);

        data[9] = 0xFC;

        SendBytes(data);
    }

    public void SendClearCommand()
    {
        byte[] data = new byte[]
        {
            0xF6, 0x00, 0x00, 0xFC
        };

        SendBytes(data);
    }

    public void SendCheckCommand()
    {
        byte[] data = new byte[]
        {
            0xF7, 0x00, 0x00, 0xFC
        };

        SendBytes(data);
    }

    void ParseMessage(string msg)
    {
        if (msg.Contains("RUN_"))
        {
            Debug.Log("运行反馈：" + msg);
        }
        else if (msg.Contains("F6 OK"))
        {
            Debug.Log("清空成功");
        }
    }

    void ParseLineCheck(byte[] data)
    {
        if (data.Length < 7) return;

        if (data[0] != 0xF2 || data[data.Length - 1] != 0xFC)
            return;

        int groupId = data[1];

        // 组合数据位
        int high = (data[2] << 8) | data[3];
        int low = (data[4] << 8) | data[5];

        int combined = (high << 16) | low;

        Debug.Log($"组 {groupId} 状态：{Convert.ToString(combined, 2)}");

        // 逐灯判断
        for (int i = 0; i < 16; i++)
        {
            bool ok = (combined & (1 << i)) != 0;

            if (!ok)
            {
                Debug.Log($"第 {groupId} 组 灯 {i} 异常");
            }
        }
    }
    
    public void SendBinFile(string filePath)
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogError("串口未打开！");
            return;
        }

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("文件不存在：" + filePath);
            return;
        }

        byte[] fileData = System.IO.File.ReadAllBytes(filePath);

        Debug.Log("开始发送BIN文件，大小：" + fileData.Length);
        UILogger.Instance?.Log($"BIN send start：{fileData.Length} byte");

        // 👉 分包发送（防止串口堵塞）
        int packetSize = 64;

        for (int i = 0; i < fileData.Length; i += packetSize)
        {
            int len = Mathf.Min(packetSize, fileData.Length - i);
            byte[] packet = new byte[len];

            Array.Copy(fileData, i, packet, 0, len);

            serialPort.Write(packet, 0, len);

            Thread.Sleep(5); // ⚠ 必须加，防止丢包
        }

        Debug.Log("BIN发送完成");
        UILogger.Instance?.Log("BIN sent successfully");
    }
}
    
