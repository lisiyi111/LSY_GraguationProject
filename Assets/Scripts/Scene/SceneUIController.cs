using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SceneUIController : MonoBehaviour
{
    public TMP_InputField inputIndex;
    public SceneBinSaver saver;
    public TMP_Dropdown dropdown;

    public int GetIndex()
    {
        int index = 0;

        if (!int.TryParse(inputIndex.text, out index))
        {
            Debug.LogError("Index输入错误");
            return 0;
        }

        return index;
    }
    

    // public void InsertScene()
    // {
    //     int index = GetIndex();
    //     saver.InsertScene(index);
    // }
    
    public void InsertScene()
    {
        int index = GetIndex();

        // ⭐ 转换为0-based
        index = Mathf.Clamp(index - 1, 0, saver.scenes.Count);

        saver.InsertScene(index);

        RefreshDropdown(); // ⭐ 插入后立即刷新UI
    }


    public void RefreshDropdown()
    {
        dropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < saver.scenes.Count; i++)
        {
            options.Add("Scene " + (i + 1));
        }

        dropdown.AddOptions(options);

        dropdown.value = 0;
        dropdown.RefreshShownValue(); // ⭐ 必须
    }

    public void ApplyFromDropdown()
    {
        int index = dropdown.value;
        saver.ApplySceneToLamps(index);
    }
}
