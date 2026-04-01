using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SFB;

public class SceneBinLoader : MonoBehaviour
{
    public SceneBinSaver saver; // 拖你的 SceneBinSaver

    public void LoadBin()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "选择BIN文件",
            "",
            "bin",
            false
        );

        if (paths.Length == 0) return;

        string path = paths[0];

        if (saver == null)
        {
            Debug.LogError("SceneBinSaver 没绑定！");
            return;
        }

        if (saver.groupLampCounts == null || saver.groupLampCounts.Length != 31)
        {
            Debug.LogError("groupLampCounts 未设置！");
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);

        saver.scenes.Clear();

        int offset = 0;

        while (offset < bytes.Length)
        {
            SceneData scene = new SceneData();

            // ⭐ 初始化 groups（必须）
            scene.groups = new Dictionary<int, List<LampData>>();
            for (int i = 1; i <= 31; i++)
                scene.groups[i] = new List<LampData>();

            for (int groupId = 1; groupId <= 31; groupId++)
            {
                // ===== ⭐ 防越界 =====
                if (offset + 2 >= bytes.Length)
                {
                    Debug.LogError("文件提前结束");
                    return;
                }

                // ===== 1. 帧头 =====
                if (bytes[offset] != 0xF5)
                {
                    Debug.LogError($"帧头错误 at {offset}");
                    return;
                }
                offset++;

                // ===== 2. groupState =====
                byte groupState = bytes[offset++];
                bool camOn = (groupState & 0b01000000) != 0;

                int lampCount = saver.groupLampCounts[groupId - 1];

                // ===== 3. 数据区 =====
                for (int i = 0; i < lampCount; i++)
                {
                    // ⭐ 防越界
                    if (offset + 8 > bytes.Length)
                    {
                        Debug.LogError("数据区越界");
                        return;
                    }

                    LampData lamp = new LampData();

                    lamp.groupId = groupId;
                    lamp.lampIndex = i;

                    lamp.R  = bytes[offset++];
                    lamp.G  = bytes[offset++];
                    lamp.B  = bytes[offset++];
                    lamp.W  = bytes[offset++];

                    lamp.R2 = bytes[offset++];
                    lamp.G2 = bytes[offset++];
                    lamp.B2 = bytes[offset++];
                    lamp.W2 = bytes[offset++];

                    lamp.cameraOn = camOn;

                    scene.groups[groupId].Add(lamp);
                }

                // ===== 4. 校验 =====
                if (offset + 2 > bytes.Length)
                {
                    Debug.LogError("校验越界");
                    return;
                }
                offset += 2;

                // ===== 5. 结束位 =====
                if (offset >= bytes.Length || bytes[offset++] != 0xFC)
                {
                    Debug.LogError("帧尾错误");
                    return;
                }
            }

            saver.scenes.Add(scene);
        }

        Debug.Log("读取完成，场景数：" + saver.scenes.Count);

        var ui = FindObjectOfType<SceneUIController>();
        if (ui != null)
            ui.RefreshDropdown();
    }
}