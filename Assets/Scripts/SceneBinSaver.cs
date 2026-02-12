using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SFB;
using System.IO;
using System.Linq;

public class SceneBinSaver : MonoBehaviour
{
    // 每组灯数量（必须与真实硬件一致）
    public int[] groupLampCounts = new int[31]
    {
        12,12,9,11,12,12,12,12,
        9,11,12,12,12,12,9,11,
        12,12,12,12,9,11,12,
        12,12,12,9,11,12,12,16
    };
    
    // ===== 按钮调用 =====
    public void SaveCurrentSceneToBin()
    {
        // 生成前，隐式提交当前正在编辑的灯
        if (LampManager.Instance != null)
        {
            LampManager.Instance.CommitCurrentLamp();
        }
        //轻提示（0.2s）
        if (CommitHint.Instance != null)
        {
            CommitHint.Instance.Show();
        }

        // 打开系统“保存文件”窗口
        var path = StandaloneFileBrowser.SaveFilePanel(
            "保存场景 BIN 文件",
            "",
            "SceneData",
            "bin"
        );

        // 用户取消
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("用户取消保存");
            return;
        }

        using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            WriteSceneData(bw);
        }

        Debug.Log("保存完成：" + path);
    }

    // ===== 原有写入逻辑（完整保留）=====
    void WriteSceneData(BinaryWriter bw)
    {
        //Lamp[] allLamps = FindObjectsOfType<Lamp>();
        
        Lamp[] allLamps = FindObjectsOfType<Lamp>(true);


        // 按组分类
        Dictionary<int, List<Lamp>> groups = new Dictionary<int, List<Lamp>>();
        for (int i = 1; i <= 31; i++)
            groups[i] = new List<Lamp>();

        foreach (var lamp in allLamps)
        {
            if (!groups.ContainsKey(lamp.groupId))
            {
                Debug.LogError($"发现一个灯的 groupId 不合法 = {lamp.groupId}（必须 1~31）");
                continue;
            }

            groups[lamp.groupId].Add(lamp);
        }

        // ===== 31 组依次写入 =====
        for (int groupId = 1; groupId <= 31; groupId++)
        {
            List<Lamp> lamps = groups[groupId];
            int lampCount = groupLampCounts[groupId - 1];

            // ===== 1. 头部 =====
            bw.Write((byte)0xF5);

            // ===== 2. 组别 + 摄像头状态 =====
            bool camState = false;
            foreach (var lamp in lamps)
            {
                if (lamp.hasCamera)
                {
                    camState = lamp.cameraOn;
                    break;
                }
            }

            byte groupState = BuildGroupStateByte(groupId, camState);
            bw.Write(groupState);

            // 校验数据（不包含 0xF5）
            List<byte> checksumData = new List<byte>();
            checksumData.Add(groupState);

            // ===== 3. RGBW 数据区 =====
            byte[] dataBlock = new byte[lampCount * 8];

            // 默认填 0
            for (int i = 0; i < dataBlock.Length; i++)
                dataBlock[i] = 0;

            // 写入灯数据（按 lampIndex）
            foreach (var lamp in lamps)
            {
                int idx = lamp.lampIndex;

                if (idx < 0 || idx >= lampCount)
                {
                    Debug.LogError($"组 {groupId} 的灯编号越界：{idx}");
                    continue;
                }

                int baseOffset = idx * 8;

                // 外圈
                dataBlock[baseOffset + 0] = (byte)lamp.R;
                dataBlock[baseOffset + 1] = (byte)lamp.G;
                dataBlock[baseOffset + 2] = (byte)lamp.B;
                dataBlock[baseOffset + 3] = (byte)lamp.W;

                // 内圈
                dataBlock[baseOffset + 4] = (byte)lamp.R2;
                dataBlock[baseOffset + 5] = (byte)lamp.G2;
                dataBlock[baseOffset + 6] = (byte)lamp.B2;
                dataBlock[baseOffset + 7] = (byte)lamp.W2;
            }

            foreach (byte b in dataBlock)
            {
                bw.Write(b);
                checksumData.Add(b);
            }

            // ===== 4. 校验和 =====
            byte checksum = CalcChecksum(checksumData);
            byte high = (byte)(checksum >> 4);
            byte low  = (byte)(checksum & 0x0F);

            bw.Write(high);
            bw.Write(low);

            // ===== 5. 结束位 =====
            bw.Write((byte)0xFC);
        }
    }

    // ===== 构造 组别 + 摄像头状态字节 =====
    byte BuildGroupStateByte(int groupId, bool cameraOn)
    {
        // bit7 固定 0
        // bit6-5 摄像头状态（10 = 开，00 = 关）
        // bit4-0 组号

        byte camBits = cameraOn ? (byte)0b01000000 : (byte)0b00000000;
        byte groupBits = (byte)(groupId & 0b00011111);

        return (byte)(camBits | groupBits);
    }

    // ===== 校验和计算 =====
    byte CalcChecksum(List<byte> data)
    {
        int sum = 0;
        foreach (byte b in data)
            sum += b;

        return (byte)(sum & 0xFF);
    }
    
}
