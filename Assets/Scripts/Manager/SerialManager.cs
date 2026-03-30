using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public class SerialManager : MonoBehaviour
{
    public static SerialManager Instance;

    public string portName = "COM3";
    public int baudRate = 115200;

    private Queue<string> msgQueue = new Queue<string>();
    private object lockObj = new object();
    
    private List<byte> bufferCache = new List<byte>();

    private SerialPort serialPort;
    private Thread receiveThread;
    private bool isRunning = false;

    void Awake()
    {
        Instance = this;
    }
    
    // void Update()
    // {
    //     lock (lockObj)
    //     {
    //         while (msgQueue.Count > 0)
    //         {
    //             string msg = msgQueue.Dequeue();
    //
    //             Debug.Log(msg);
    //             UILogger.Instance?.Log(msg);
    //         }
    //     }
    // }
    
    void Update()
    {
        int count = 0;

        lock (lockObj)
        {
            while (msgQueue.Count > 0 && count < 5)
            {
                string msg = msgQueue.Dequeue();

                Debug.Log(msg);
                UILogger.Instance?.Log(msg);

                count++;
            }
        }
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
            serialPort.ReadTimeout = 50;
            serialPort.Open();

            isRunning = true;
            receiveThread = new Thread(ReadData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("串口打开成功");
            UILogger.Instance?.Log("Serial port opened");
        }
        catch (Exception e)
        {
            Debug.LogError("串口打开失败：" + e.Message);
            UILogger.Instance?.Log("Open failed: " + e.Message);
        }
    }

    // ===== 关闭串口 =====
    public void ClosePort()
    {
        isRunning = false;

        if (receiveThread != null)
            receiveThread.Join();   // ✅ 不再用Abort

        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();

        Debug.Log("串口已关闭");
        UILogger.Instance?.Log("Serial port closed");
    }

    // ===== 发送数据 =====
    public void SendBytes(byte[] data)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write(data, 0, data.Length);

            string msg = "TX: " + BitConverter.ToString(data);
            Debug.Log(msg);
            UILogger.Instance?.Log(msg);
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
                    int count = serialPort.BytesToRead;

                    if (count > 0)
                    {
                        byte[] buffer = new byte[count];
                        serialPort.Read(buffer, 0, count);

                        lock (lockObj)
                        {
                            // ===== HEX显示 =====
                            msgQueue.Enqueue("HEX: " + BitConverter.ToString(buffer));

                            // ===== 字符串显示 =====
                            string text = System.Text.Encoding.UTF8.GetString(buffer);

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                msgQueue.Enqueue("STR: " + text);

                                // ⭐⭐⭐ 关键：直接解析字符串协议
                                ParseTextProtocol(text);
                            }

                            // ===== ⭐ 加入缓存 =====
                            bufferCache.AddRange(buffer);

                            // ===== ⭐ 尝试解析 =====
                            TryParseBuffer();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                lock (lockObj)
                {
                    msgQueue.Enqueue("Error: " + e.Message);
                }
            }

            Thread.Sleep(10);
        }
    }
    
    void ParseTextProtocol(string text)
    {
        if (text.Contains("RUN_"))
        {
            msgQueue.Enqueue("Feedback on performance : " + text);
        }

        if (text.Contains("F6 OK"))
        {
            msgQueue.Enqueue("Clearing successful");
        }
    }
    
    void TryParseBuffer()
    {
        while (bufferCache.Count >= 7)
        {
            if (bufferCache[0] != 0xF2)
            {
                bufferCache.RemoveAt(0);
                continue;
            }

            int packetLength = 7;

            if (bufferCache.Count < packetLength)
                return;

            byte[] packet = bufferCache.GetRange(0, packetLength).ToArray();

            if (packet[6] != 0xFC)
            {
                bufferCache.RemoveAt(0);
                continue;
            }

            bufferCache.RemoveRange(0, packetLength);

            //msgQueue.Enqueue("Capture complete F2 frames");

            ParseProtocol(packet);
        }
    }
    
    
    void ParseProtocol(byte[] data)
    {
        if (data.Length == 7 && data[0] == 0xF2 && data[6] == 0xFC)
        {
            //msgQueue.Enqueue("Identified as a line detection frame");

            ParseLineCheck(data);
        }

        // ===== 字符串反馈 =====
        string text = System.Text.Encoding.UTF8.GetString(data);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[\x00-\x1F]", "");
        
    }
    

    // ===== 发送命令 =====
    public void SendRunCommand(int gap, int sceneCount, int loop)
    {
        byte[] data = new byte[10];

        data[0] = 0xF8;
        data[1] = (byte)(gap / 10);
        data[2] = (byte)(gap % 10);

        data[3] = (byte)((sceneCount >> 12) & 0x0F);
        data[4] = (byte)((sceneCount >> 8) & 0x0F);
        data[5] = (byte)((sceneCount >> 4) & 0x0F);
        data[6] = (byte)(sceneCount & 0x0F);

        data[7] = (byte)(loop / 10);
        data[8] = (byte)(loop % 10);

        data[9] = 0xFC;

        SendBytes(data);
    }

    public void SendClearCommand()
    {
        byte[] data = new byte[] { 0xF6, 0x00, 0x00, 0xFC }; 
        SendBytes(data);
    }

    public void SendCheckCommand()
    {
        byte[] data = new byte[] { 0xF7, 0x00, 0x00, 0xFC }; 
        SendBytes(data);
    }
    

    
    void ParseLineCheck(byte[] data)
    {
        int groupId = data[1];

        // 拼接四个4bit → 16bit
        int value =
            (data[2] << 12) |
            (data[3] << 8) |
            (data[4] << 4) |
            data[5];

        string bin = Convert.ToString(value, 2).PadLeft(16, '0');
        msgQueue.Enqueue($"Group {groupId} state: {bin}");

        // 获取灯数量，减去0号灯
        int ledCount = GetLedCount(groupId) - 1;

        for (int lampIndex = 1; lampIndex <= ledCount; lampIndex++)
        {
            // 二进制最右边是1号灯，对应 lampIndex - 1
            bool isNormal = (value & (1 << (lampIndex - 1))) != 0;

            // if (isNormal)
            //     msgQueue.Enqueue($"Group {groupId} Lamp {lampIndex}: normal");
            // else
            //     msgQueue.Enqueue($"Group {groupId} Lamp {lampIndex}: error");
            
            if (!isNormal)
                msgQueue.Enqueue($"Group {groupId} Lamp {lampIndex}: error");
        }
    }
    
    int GetLedCount(int groupId)
    {
        // 12个
        int[] group12 = {1,2,5,6,7,8,11,12,13,14,17,18,19,20,23,24,25,26,29,30};

        // 11个
        int[] group11 = {4,10,16,22,28};

        // 9个
        int[] group9 = {3,9,15,21,27};

        if (Array.Exists(group12, g => g == groupId)) return 12;
        if (Array.Exists(group11, g => g == groupId)) return 11;
        if (Array.Exists(group9, g => g == groupId)) return 9;

        if (groupId == 31) return 16;

        return 12; // 默认
    }

    public void SendBinFile(string filePath)
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            Debug.LogError("串口未打开！");
            return;
        }

        byte[] fileData = System.IO.File.ReadAllBytes(filePath);

        UILogger.Instance?.Log($"BIN send start: {fileData.Length} bytes");

        int packetSize = 64;

        for (int i = 0; i < fileData.Length; i += packetSize)
        {
            int len = Mathf.Min(packetSize, fileData.Length - i);

            byte[] packet = new byte[len];
            Array.Copy(fileData, i, packet, 0, len);

            serialPort.Write(packet, 0, len);

            Thread.Sleep(5);
        }

        UILogger.Instance?.Log("BIN send done");
    }
}