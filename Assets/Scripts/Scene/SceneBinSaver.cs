using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SFB;
using System.IO;
using System.Linq;
using TMPro;

public class SceneBinSaver : MonoBehaviour
{
    [Header("Scene List")]
    public List<SceneData> scenes = new List<SceneData>();

    [Header("Generate Confirm UI")]
    public GameObject generateConfirmPanel;
    public TMP_Text generateConfirmText;
    
    // 每组灯数量（必须与真实硬件一致）
    public int[] groupLampCounts = new int[31]
    {
        12,12,9,11,12,12,12,12,
        9,11,12,12,12,12,9,11,
        12,12,12,12,9,11,12,
        12,12,12,9,11,12,12,16
    };
    
    void Start()
    {
        if (generateConfirmPanel != null)
            generateConfirmPanel.SetActive(false);
    }

    // ===== 按钮调用 =====
    public void SaveCurrentSceneToBin()
    {
        // 有弹窗就先确认；没有弹窗则按原逻辑直接保存
        if (generateConfirmPanel != null)
        {
            ShowGenerateConfirm();
            return;
        }

        ExecuteSaveCurrentSceneToBin();
    }

    // ===== 生成前确认 =====
    void ShowGenerateConfirm()
    {
        if (generateConfirmText != null)
        {
            generateConfirmText.text = $"是否保存当前场景？\n当前场景数为（{scenes.Count}）";
        }

        generateConfirmPanel.SetActive(true);
    }

    // 点击“是”：先Add当前场景，再生成
    public void ConfirmGenerateAddCurrentScene()
    {
        if (generateConfirmPanel != null)
            generateConfirmPanel.SetActive(false);

        AddCurrentScene();
        ExecuteSaveCurrentSceneToBin();
    }

    // 点击“否”：不Add当前场景，直接生成
    public void ConfirmGenerateWithoutAdd()
    {
        if (generateConfirmPanel != null)
            generateConfirmPanel.SetActive(false);

        ExecuteSaveCurrentSceneToBin();
    }

    // 点击“取消”：关闭弹窗，不进行生成
    public void CancelGenerateConfirm()
    {
        if (generateConfirmPanel != null)
            generateConfirmPanel.SetActive(false);
    }

    void ExecuteSaveCurrentSceneToBin()
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
            //WriteSceneData(bw);
            
            foreach (var scene in scenes)
            {
                WriteOneScene(bw, scene);
            }
        }

        Debug.Log("保存完成：" + path);
    }
    
    void WriteOneScene(BinaryWriter bw, SceneData scene)
    {
        for (int groupId = 1; groupId <= 31; groupId++)
        {
            if (!scene.groups.ContainsKey(groupId))
            {
                scene.groups[groupId] = new List<LampData>();
            }

            List<LampData> lamps = scene.groups[groupId];
            int lampCount = groupLampCounts[groupId - 1];

            // 1. 头
            bw.Write((byte)0xF5);

            // 2. 摄像头
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

            List<byte> checksumData = new List<byte>();
            checksumData.Add(groupState);

            // 3. 数据区
            byte[] dataBlock = new byte[lampCount * 8];

            foreach (var lamp in lamps)
            {
                int idx = lamp.lampIndex;
                if (idx < 0 || idx >= lampCount) continue;

                int baseOffset = idx * 8;

                dataBlock[baseOffset + 0] = lamp.R;
                dataBlock[baseOffset + 1] = lamp.G;
                dataBlock[baseOffset + 2] = lamp.B;
                dataBlock[baseOffset + 3] = lamp.W;

                dataBlock[baseOffset + 4] = lamp.R2;
                dataBlock[baseOffset + 5] = lamp.G2;
                dataBlock[baseOffset + 6] = lamp.B2;
                dataBlock[baseOffset + 7] = lamp.W2;
            }

            foreach (byte b in dataBlock)
            {
                bw.Write(b);
                checksumData.Add(b);
            }

            // 4. 校验
            byte checksum = CalcChecksum(checksumData);
            bw.Write((byte)(checksum >> 4));
            bw.Write((byte)(checksum & 0x0F));

            // 5. 结束
            bw.Write((byte)0xFC);
        }
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
    
    public void AddCurrentScene()
    {
        // 提交当前编辑
        if (LampManager.Instance != null)
            LampManager.Instance.CommitCurrentLamp();

        Lamp[] allLamps = FindObjectsOfType<Lamp>(true);

        SceneData scene = new SceneData();

        // 初始化31组
        for (int i = 1; i <= 31; i++)
            scene.groups[i] = new List<LampData>();

        foreach (var lamp in allLamps)
        {
            LampData data = new LampData()
            {
                groupId = lamp.groupId,
                lampIndex = lamp.lampIndex,

                R = (byte)lamp.R,
                G = (byte)lamp.G,
                B = (byte)lamp.B,
                W = (byte)lamp.W,

                R2 = (byte)lamp.R2,
                G2 = (byte)lamp.G2,
                B2 = (byte)lamp.B2,
                W2 = (byte)lamp.W2,

                hasCamera = lamp.hasCamera,
                cameraOn = lamp.cameraOn
            };

            scene.groups[lamp.groupId].Add(data);
        }

        scenes.Add(scene);

        Debug.Log("添加场景成功，总数：" + scenes.Count);
    }
    
    
    public void ApplySceneToLamps(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= scenes.Count) return;

        SceneData scene = scenes[sceneIndex];

        Lamp[] allLamps = FindObjectsOfType<Lamp>(true);

        foreach (var lamp in allLamps)
        {
            var list = scene.groups[lamp.groupId];

            foreach (var data in list)
            {
                if (data.lampIndex == lamp.lampIndex)
                {
                    lamp.SetR(data.R);
                    lamp.SetG(data.G);
                    lamp.SetB(data.B);
                    lamp.SetW(data.W);

                    lamp.SetR2(data.R2);
                    lamp.SetG2(data.G2);
                    lamp.SetB2(data.B2);
                    lamp.SetW2(data.W2);

                    lamp.SetCameraState(data.cameraOn);
                    break;
                }
            }
        }

        Debug.Log("已应用场景：" + (sceneIndex + 1 ));
    }
    
    public void InsertScene(int index)
    {
        if (LampManager.Instance != null)
            LampManager.Instance.CommitCurrentLamp();

        SceneData scene = CaptureCurrentScene();

        if (index < 0 || index > scenes.Count)
        {
            Debug.LogError("插入位置非法：" + index);
            return;
        }

        scenes.Insert(index, scene);

        Debug.Log("插入成功，当前场景数：" + scenes.Count);
    }
    
    SceneData CaptureCurrentScene()
    {
        Lamp[] allLamps = FindObjectsOfType<Lamp>(true);

        SceneData scene = new SceneData();

        // ⭐⭐⭐ 必须初始化
        scene.groups = new Dictionary<int, List<LampData>>();
        for (int i = 1; i <= 31; i++)
            scene.groups[i] = new List<LampData>();

        foreach (var lamp in allLamps)
        {
            LampData data = new LampData()
            {
                groupId = lamp.groupId,
                lampIndex = lamp.lampIndex,

                R = (byte)lamp.R,
                G = (byte)lamp.G,
                B = (byte)lamp.B,
                W = (byte)lamp.W,

                R2 = (byte)lamp.R2,
                G2 = (byte)lamp.G2,
                B2 = (byte)lamp.B2,
                W2 = (byte)lamp.W2,

                hasCamera = lamp.hasCamera,
                cameraOn = lamp.cameraOn
            };

            scene.groups[lamp.groupId].Add(data);
        }

        return scene;
    }
    
    
}

