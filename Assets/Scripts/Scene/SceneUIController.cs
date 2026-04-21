using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using SFB;

public class SceneUIController : MonoBehaviour
{
    public TMP_InputField inputIndex;
    public SceneBinSaver saver;
    public SceneBinLoader loader;
    public TMP_Dropdown dropdown;

    [Header("仅插入场景：输入位置校验提示（不要用于其它功能）")]
    public TMP_Text insertWarningText;
    public float warningDuration = 2f;
    private Coroutine warningCoroutine;

    [Header("编辑场景时 — 拦截其它操作的提示（勿用 insertWarning）")]
    public TMP_Text editBlockingHintText;
    public float editBlockHintDuration = 2.5f;
    private Coroutine editBlockCoroutine;

    [Header("Merge BIN into current list")]
    public GameObject mergePositionPanel;
    private string pendingMergeBinPath;

    [Header("Load overwrite confirm")]
    public GameObject loadOverwritePanel;
    public TMP_Text loadOverwriteMessage;

    [Header("Edit dropdown scene mode")]
    public TMP_Text editModeHintText;
    [Tooltip("可选：编辑按钮上的 TMP 文案，进入编辑后显示「返回」")]
    public TMP_Text editSceneButtonLabel;
    public string editSceneButtonCaptionIdle = "编辑当前场景";
    public string editSceneButtonCaptionEditing = "返回";

    private bool isEditingDropdownScene;
    private int lastDropdownValue = -1;
    private bool suppressDropdownSync;

    /// <summary>灯在「非编辑列表项」模式下被改过，且未通过添加/插入/下拉应用等写回列表时，生成 BIN 前要弹窗。</summary>
    private bool lampEditsNeedAddBeforeGenerate;
    private bool suppressLampDirtyTracking;

    public bool ShouldConfirmBeforeGenerate()
    {
        if (isEditingDropdownScene) return false;
        return lampEditsNeedAddBeforeGenerate;
    }

    public void MarkLampEditsCommittedToList()
    {
        lampEditsNeedAddBeforeGenerate = false;
    }

    public const string EditingSceneBlockMessage = "正在编辑当前场景，进行该操作需要点击'返回'";

    public static SceneUIController Instance { get; private set; }

    public bool IsEditingDropdownScene => isEditingDropdownScene;

    /// <summary>编辑模式下重置灯后，把当前列表项与场上灯状态对齐。</summary>
    public void RefreshEditingSceneFromCurrentLamps()
    {
        if (!isEditingDropdownScene || saver == null || dropdown == null) return;
        int idx = dropdown.value;
        if (idx < 0 || idx >= saver.scenes.Count) return;
        saver.scenes[idx] = saver.CaptureCurrentScene();
    }

    void Awake()
    {
        Instance = this;
    }

    /// <summary>若正在「编辑当前场景」，提示并返回 true（应直接 return 不再执行原逻辑）。</summary>
    public bool BlockIfEditingScene()
    {
        if (!isEditingDropdownScene) return false;
        ShowEditBlockingHint(EditingSceneBlockMessage);
        return true;
    }

    void ShowEditBlockingHint(string msg)
    {
        if (editBlockingHintText == null)
        {
            Debug.LogWarning(msg);
            return;
        }

        editBlockingHintText.text = msg;
        editBlockingHintText.gameObject.SetActive(true);

        if (editBlockCoroutine != null)
            StopCoroutine(editBlockCoroutine);
        editBlockCoroutine = StartCoroutine(HideEditBlockingLater());
    }

    IEnumerator HideEditBlockingLater()
    {
        yield return new WaitForSeconds(editBlockHintDuration);
        if (editBlockingHintText != null)
            editBlockingHintText.gameObject.SetActive(false);
        editBlockCoroutine = null;
    }

    void Start()
    {
        if (insertWarningText != null)
            insertWarningText.gameObject.SetActive(false);
        if (editBlockingHintText != null)
            editBlockingHintText.gameObject.SetActive(false);
        if (mergePositionPanel != null)
            mergePositionPanel.SetActive(false);
        if (loadOverwritePanel != null)
            loadOverwritePanel.SetActive(false);

        if (dropdown != null)
            dropdown.onValueChanged.AddListener(OnDropdownSelectionChanged);

        if (LampManager.Instance != null)
            LampManager.Instance.OnLampValuesChanged += OnLampValuesChangedFromUI;

        UpdateEditModeHint();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownSelectionChanged);

        if (LampManager.Instance != null)
            LampManager.Instance.OnLampValuesChanged -= OnLampValuesChangedFromUI;
    }

    void OnDropdownSelectionChanged(int newIdx)
    {
        if (suppressDropdownSync)
        {
            lastDropdownValue = newIdx;
            return;
        }

        if (!isEditingDropdownScene)
        {
            lastDropdownValue = newIdx;
            if (saver != null && newIdx >= 0 && newIdx < saver.scenes.Count)
            {
                suppressLampDirtyTracking = true;
                try
                {
                    saver.ApplySceneToLamps(newIdx);
                }
                finally
                {
                    suppressLampDirtyTracking = false;
                }
                MarkLampEditsCommittedToList();
            }
            return;
        }

        if (saver == null) return;

        if (lastDropdownValue >= 0 && lastDropdownValue < saver.scenes.Count)
            saver.scenes[lastDropdownValue] = saver.CaptureCurrentScene();

        if (newIdx >= 0 && newIdx < saver.scenes.Count)
        {
            suppressLampDirtyTracking = true;
            try
            {
                saver.ApplySceneToLamps(newIdx);
            }
            finally
            {
                suppressLampDirtyTracking = false;
            }
        }

        lastDropdownValue = newIdx;
    }

    /// <summary>将当前下拉选中项应用到灯（加载/刷新列表后也会调用）。</summary>
    void ApplyCurrentSelectionToLamps()
    {
        if (saver == null || dropdown == null || saver.scenes.Count == 0) return;

        int v = Mathf.Clamp(dropdown.value, 0, saver.scenes.Count - 1);
        suppressLampDirtyTracking = true;
        try
        {
            saver.ApplySceneToLamps(v);
        }
        finally
        {
            suppressLampDirtyTracking = false;
        }
        lastDropdownValue = v;
        MarkLampEditsCommittedToList();
    }

    void OnLampValuesChangedFromUI()
    {
        if (suppressLampDirtyTracking) return;

        if (isEditingDropdownScene && saver != null && dropdown != null)
        {
            int idx = dropdown.value;
            if (idx >= 0 && idx < saver.scenes.Count)
                saver.scenes[idx] = saver.CaptureCurrentScene();
            return;
        }

        lampEditsNeedAddBeforeGenerate = true;
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

    public void OnClickAddScene()
    {
        if (saver == null || dropdown == null) return;
        if (BlockIfEditingScene()) return;
        saver.AddCurrentScene();
        RefreshDropdown();
        if (saver.scenes.Count > 0)
        {
            dropdown.value = saver.scenes.Count - 1;
            dropdown.RefreshShownValue();
            lastDropdownValue = dropdown.value;
        }
    }

    public void OnClickInsertScene()
    {
        if (saver == null || dropdown == null) return;
        if (BlockIfEditingScene()) return;

        if (!TryGetValidInsertIndex(out int index))
            return;

        saver.InsertScene(index);
        RefreshDropdown();
        dropdown.value = index;
        dropdown.RefreshShownValue();
        lastDropdownValue = index;
    }

    public void InsertScene()
    {
        OnClickInsertScene();
    }

    public void OnClickLoadSceneBin()
    {
        if (loader == null || saver == null) return;
        if (BlockIfEditingScene()) return;

        if (saver.scenes.Count > 0 && loadOverwritePanel != null)
        {
            if (loadOverwriteMessage != null)
                loadOverwriteMessage.text = "当前操作会覆盖已编辑场景，是否继续？";
            loadOverwritePanel.SetActive(true);
            return;
        }

        loader.LoadBin();
        lastDropdownValue = dropdown != null ? dropdown.value : -1;
    }

    public void LoadOverwriteCancel()
    {
        if (loadOverwritePanel != null)
            loadOverwritePanel.SetActive(false);
    }

    public void LoadOverwriteSaveCurrent()
    {
        if (BlockIfEditingScene()) return;

        if (loadOverwritePanel != null)
            loadOverwritePanel.SetActive(false);

        if (saver != null)
            saver.SaveCurrentSceneToBinDirect();
    }

    public void LoadOverwriteConfirm()
    {
        if (BlockIfEditingScene()) return;

        if (loadOverwritePanel != null)
            loadOverwritePanel.SetActive(false);

        if (loader != null)
            loader.LoadBin();

        if (dropdown != null)
            lastDropdownValue = dropdown.value;
    }

    public void OnClickGenerateSceneBin()
    {
        if (saver == null) return;
        if (BlockIfEditingScene()) return;
        saver.SaveCurrentSceneToBin();
    }

    /// <summary>选择 BIN 后弹出“最前/最后”，将文件中全部场景合并进当前列表。</summary>
    public void OnClickChooseFileToMerge()
    {
        if (saver == null || loader == null) return;
        if (BlockIfEditingScene()) return;

        var paths = StandaloneFileBrowser.OpenFilePanel("选择要合并的BIN文件", "", "bin", false);
        if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
            return;

        pendingMergeBinPath = paths[0];

        if (mergePositionPanel != null)
            mergePositionPanel.SetActive(true);
        else
            Debug.LogWarning("请绑定 mergePositionPanel（含 最前/最后 按钮）");
    }

    public void MergePendingAtFront()
    {
        if (BlockIfEditingScene()) return;
        DoMergeFile(prepend: true);
    }

    public void MergePendingAtBack()
    {
        if (BlockIfEditingScene()) return;
        DoMergeFile(prepend: false);
    }

    public void MergePendingCancel()
    {
        pendingMergeBinPath = null;
        if (mergePositionPanel != null)
            mergePositionPanel.SetActive(false);
    }

    void DoMergeFile(bool prepend)
    {
        if (string.IsNullOrEmpty(pendingMergeBinPath) || saver == null || loader == null || dropdown == null)
        {
            MergePendingCancel();
            return;
        }

        if (BlockIfEditingScene())
        {
            MergePendingCancel();
            return;
        }

        byte[] bytes = File.ReadAllBytes(pendingMergeBinPath);
        List<SceneData> parsed = loader.TryParseBin(bytes);
        MergePendingCancel();

        if (parsed == null || parsed.Count == 0)
        {
            Debug.LogWarning("文件解析失败或没有场景");
            return;
        }

        if (prepend)
            saver.scenes.InsertRange(0, parsed);
        else
            saver.scenes.AddRange(parsed);

        RefreshDropdown();
        if (saver.scenes.Count > 0)
        {
            dropdown.value = prepend ? 0 : saver.scenes.Count - 1;
            dropdown.RefreshShownValue();
            lastDropdownValue = dropdown.value;
        }

        Debug.Log($"合并完成，当前场景数：{saver.scenes.Count}");
    }

    /// <summary>按钮：切换“编辑当前下拉选中场景”。</summary>
    public void ToggleEditDropdownSceneMode()
    {
        if (isEditingDropdownScene)
            ExitEditDropdownSceneMode();
        else
            EnterEditDropdownSceneMode();
    }

    /// <summary>Toggle 组件可绑定：勾选=进入编辑，取消=退出。</summary>
    public void OnEditSceneToggle(bool on)
    {
        if (on)
            EnterEditDropdownSceneMode();
        else
            ExitEditDropdownSceneMode();
    }

    void EnterEditDropdownSceneMode()
    {
        if (saver == null || dropdown == null || saver.scenes.Count == 0)
        {
            Debug.LogWarning("没有可编辑的场景");
            return;
        }

        LampManager.Instance?.CommitCurrentLamp();

        isEditingDropdownScene = true;
        lastDropdownValue = dropdown.value;
        if (lastDropdownValue >= 0 && lastDropdownValue < saver.scenes.Count)
        {
            suppressLampDirtyTracking = true;
            try
            {
                saver.ApplySceneToLamps(lastDropdownValue);
            }
            finally
            {
                suppressLampDirtyTracking = false;
            }
        }

        MarkLampEditsCommittedToList();
        UpdateEditModeHint();
    }

    void ExitEditDropdownSceneMode()
    {
        if (!isEditingDropdownScene) return;

        LampManager.Instance?.CommitCurrentLamp();

        if (saver != null && lastDropdownValue >= 0 && lastDropdownValue < saver.scenes.Count)
            saver.scenes[lastDropdownValue] = saver.CaptureCurrentScene();

        isEditingDropdownScene = false;
        MarkLampEditsCommittedToList();
        UpdateEditModeHint();
    }

    void UpdateEditModeHint()
    {
        if (editModeHintText != null)
        {
            editModeHintText.text = isEditingDropdownScene ? "编辑场景中..." : "";
            editModeHintText.gameObject.SetActive(isEditingDropdownScene);
        }

        if (editSceneButtonLabel != null)
            editSceneButtonLabel.text = isEditingDropdownScene
                ? editSceneButtonCaptionEditing
                : editSceneButtonCaptionIdle;
    }

    public void RefreshDropdown()
    {
        if (dropdown == null || saver == null) return;

        suppressDropdownSync = true;
        try
        {
            dropdown.ClearOptions();

            List<string> options = new List<string>();

            for (int i = 0; i < saver.scenes.Count; i++)
                options.Add("Scene " + (i + 1));

            if (options.Count == 0)
            {
                lastDropdownValue = -1;
                return;
            }

            dropdown.AddOptions(options);

            dropdown.value = Mathf.Clamp(dropdown.value, 0, options.Count - 1);
            dropdown.RefreshShownValue();
            lastDropdownValue = dropdown.value;
        }
        finally
        {
            suppressDropdownSync = false;
        }

        if (saver.scenes.Count > 0)
            ApplyCurrentSelectionToLamps();
    }

    /// <summary>兼容旧按钮绑定；平时选下拉即可自动应用。</summary>
    public void ApplyFromDropdown()
    {
        if (saver == null || dropdown == null) return;
        if (BlockIfEditingScene()) return;
        if (saver.scenes.Count == 0)
        {
            Debug.LogWarning("当前没有可应用的场景");
            return;
        }

        if (isEditingDropdownScene)
        {
            int prev = lastDropdownValue;
            if (prev >= 0 && prev < saver.scenes.Count)
                saver.scenes[prev] = saver.CaptureCurrentScene();
        }

        ApplyCurrentSelectionToLamps();
    }
    // ==========================
// 【新增】删除当前选中的场景
// ==========================
    public void OnClickDeleteScene()
    {
        if (saver == null || dropdown == null)
        {
            Debug.LogWarning("未绑定 saver 或 dropdown");
            return;
        }

        // 1. 如果正在编辑场景，禁止删除
        if (BlockIfEditingScene())
            return;

        // 2. 检查是否有场景可删
        if (saver.scenes.Count == 0)
        {
            ShowInsertWarning("场景列表为空，无法删除！");
            return;
        }

        // 3. 检查当前选中是否合法
        int selectedIndex = dropdown.value;
        if (selectedIndex < 0 || selectedIndex >= saver.scenes.Count)
        {
            ShowInsertWarning("请先选择要删除的场景！");
            return;
        }

        // 4. 执行删除
        saver.scenes.RemoveAt(selectedIndex);
        Debug.Log($"已删除场景：{selectedIndex + 1}");

        // 5. 刷新下拉列表
        RefreshDropdown();

        // 6. 如果删完还有场景，自动选中第一个
        if (saver.scenes.Count > 0)
        {
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            lastDropdownValue = 0;
            ApplyCurrentSelectionToLamps();
        }
    }
}
