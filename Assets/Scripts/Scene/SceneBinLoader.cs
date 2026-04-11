using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SFB;

public class SceneBinLoader : MonoBehaviour
{
    public SceneBinSaver saver;

    public void LoadBin()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "选择BIN文件",
            "",
            "bin",
            false
        );

        if (paths.Length == 0) return;

        LoadBinFromPath(paths[0]);
    }

    /// <summary>从路径加载并替换内存中的全部场景（不弹文件框）。</summary>
    public void LoadBinFromPath(string path)
    {
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
        List<SceneData> parsed = TryParseBin(bytes);
        if (parsed == null)
            return;

        saver.scenes.Clear();
        saver.scenes.AddRange(parsed);

        Debug.Log("读取完成，场景数：" + saver.scenes.Count);

        var ui = FindObjectOfType<SceneUIController>();
        if (ui != null)
            ui.RefreshDropdown();
    }

    /// <summary>解析 BIN 为场景列表；失败返回 null。</summary>
    public List<SceneData> TryParseBin(byte[] bytes)
    {
        if (saver == null || saver.groupLampCounts == null || saver.groupLampCounts.Length != 31)
        {
            Debug.LogError("SceneBinSaver 或 groupLampCounts 无效");
            return null;
        }

        var result = new List<SceneData>();
        int offset = 0;

        while (offset < bytes.Length)
        {
            SceneData scene = new SceneData();
            scene.groups = new Dictionary<int, List<LampData>>();
            for (int i = 1; i <= 31; i++)
                scene.groups[i] = new List<LampData>();

            for (int groupId = 1; groupId <= 31; groupId++)
            {
                if (offset + 2 >= bytes.Length)
                {
                    Debug.LogError("文件提前结束");
                    return null;
                }

                if (bytes[offset] != 0xF5)
                {
                    Debug.LogError($"帧头错误 at {offset}");
                    return null;
                }
                offset++;

                byte groupState = bytes[offset++];
                bool camOn = (groupState & 0b01000000) != 0;

                int lampCount = saver.groupLampCounts[groupId - 1];

                for (int i = 0; i < lampCount; i++)
                {
                    if (offset + 8 > bytes.Length)
                    {
                        Debug.LogError("数据区越界");
                        return null;
                    }

                    LampData lamp = new LampData
                    {
                        groupId = groupId,
                        lampIndex = i,
                        R = bytes[offset++],
                        G = bytes[offset++],
                        B = bytes[offset++],
                        W = bytes[offset++],
                        R2 = bytes[offset++],
                        G2 = bytes[offset++],
                        B2 = bytes[offset++],
                        W2 = bytes[offset++],
                        cameraOn = camOn
                    };

                    scene.groups[groupId].Add(lamp);
                }

                if (offset + 2 > bytes.Length)
                {
                    Debug.LogError("校验越界");
                    return null;
                }
                offset += 2;

                if (offset >= bytes.Length || bytes[offset++] != 0xFC)
                {
                    Debug.LogError("帧尾错误");
                    return null;
                }
            }

            result.Add(scene);
        }

        return result;
    }
}
