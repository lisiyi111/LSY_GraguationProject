using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SceneUIController : MonoBehaviour
{
    public TMP_InputField inputIndex;
    public SceneBinSaver saver;
    public SceneBinLoader loader;
    public TMP_Dropdown dropdown;
    [Header("Insert Warning UI")]
    public TMP_Text insertWarningText;
    public float warningDuration = 2f;
    private Coroutine warningCoroutine;

    void Start()
    {
        if (insertWarningText != null)
            insertWarningText.gameObject.SetActive(false);
    }

    bool TryGetValidInsertIndex(out int zeroBasedIndex)
    {
        zeroBasedIndex = -1;
        string raw = inputIndex != null ? inputIndex.text.Trim() : "";

        if (string.IsNullOrEmpty(raw))
        {
            ShowInsertWarning("请输入插入位置");
            return false;
        }

        if (!int.TryParse(raw, out int oneBasedIndex))
        {
            ShowInsertWarning("插入位置必须是数字");
            return false;
        }

        int min = 1;
        int max = saver != null ? saver.scenes.Count + 1 : 1;

        if (oneBasedIndex < min || oneBasedIndex > max)
        {
            ShowInsertWarning($"插入位置范围：{min}~{max}");
            return false;
        }

        zeroBasedIndex = oneBasedIndex - 1;
        return true;
    }

    void ShowInsertWarning(string msg)
    {
        Debug.LogWarning(msg);
        if (insertWarningText == null) return;

        insertWarningText.text = msg;
        insertWarningText.gameObject.SetActive(true);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(HideWarningLater());
    }

    IEnumerator HideWarningLater()
    {
        yield return new WaitForSeconds(warningDuration);
        if (insertWarningText != null)
            insertWarningText.gameObject.SetActive(false);
        warningCoroutine = null;
    }

    // ===== 推荐绑定：添加场景按钮 =====
    public void OnClickAddScene()
    {
        if (saver == null) return;
        saver.AddCurrentScene();
        RefreshDropdown();
        dropdown.value = saver.scenes.Count - 1;
        dropdown.RefreshShownValue();
    }

    // ===== 推荐绑定：插入场景按钮 =====
    public void OnClickInsertScene()
    {
        if (saver == null) return;

        if (!TryGetValidInsertIndex(out int index))
            return;

        saver.InsertScene(index);
        RefreshDropdown();
        dropdown.value = index;
        dropdown.RefreshShownValue();
    }

    // 兼容旧按钮绑定
    public void InsertScene()
    {
        OnClickInsertScene();
    }

    // ===== 推荐绑定：加载场景按钮 =====
    public void OnClickLoadSceneBin()
    {
        if (loader == null) return;
        loader.LoadBin();
    }

    // ===== 推荐绑定：生成场景按钮 =====
    public void OnClickGenerateSceneBin()
    {
        if (saver == null) return;
        saver.SaveCurrentSceneToBin();
    }

    public void RefreshDropdown()
    {
        if (dropdown == null || saver == null) return;

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < saver.scenes.Count; i++)
        {
            options.Add("Scene " + (i + 1));
        }

        if (options.Count == 0) return;

        dropdown.AddOptions(options);

        dropdown.value = Mathf.Clamp(dropdown.value, 0, options.Count - 1);
        dropdown.RefreshShownValue(); // ⭐ 必须
    }

    public void ApplyFromDropdown()
    {
        if (saver == null || dropdown == null) return;
        if (saver.scenes.Count == 0)
        {
            ShowInsertWarning("当前没有可应用的场景");
            return;
        }

        int index = dropdown.value;
        saver.ApplySceneToLamps(index);
    }
}
