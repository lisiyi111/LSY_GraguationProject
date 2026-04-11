using UnityEngine;

public class UIPanelToggle : MonoBehaviour
{
    public GameObject panel;

    [Tooltip("勾选后：编辑当前场景时禁止「打开」该面板（关闭仍允许）。用于包住重置位置/重置参数的总重置按钮。")]
    public bool blockOpenWhenEditingScene;

    private bool isOpen = false;

    void Start()
    {
        panel.SetActive(isOpen);

    }
    public void TogglePanel()
    {
        bool willOpen = !isOpen;
        if (willOpen && blockOpenWhenEditingScene &&
            SceneUIController.Instance != null &&
            SceneUIController.Instance.BlockIfEditingScene())
            return;

        isOpen = willOpen;
        panel.SetActive(isOpen);
    }
}
