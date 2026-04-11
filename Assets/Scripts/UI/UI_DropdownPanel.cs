// using UnityEngine;

// public class UI_DropdownPanel : MonoBehaviour
// {
//     public GameObject panel;
//
//     private bool isOpen = false;
//
//     void Start()
//     {
//         panel.SetActive(false); // ⭐ 开局隐藏
//     }
//
//     public void Toggle()
//     {
//         isOpen = !isOpen;
//         panel.SetActive(isOpen);
//     }
// }

using UnityEngine;

public class UI_DropdownPanel : MonoBehaviour
{
    public RectTransform panel;
    public float speed = 10f;

    [Tooltip("勾选后：编辑当前场景时禁止滑出打开（关闭仍允许）")]
    public bool blockOpenWhenEditingScene;

    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isOpen = false;

    void Start()
    {
        shownPos = new Vector2(panel.anchoredPosition.x,0);
        hiddenPos = new Vector2(panel.anchoredPosition.x,panel.rect.height);

        panel.anchoredPosition = hiddenPos;
    }

    void Update()
    {
        Vector2 target = isOpen ? shownPos : hiddenPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );
    }

    public void TogglePanel()
    {
        bool willOpen = !isOpen;
        if (willOpen && blockOpenWhenEditingScene &&
            SceneUIController.Instance != null &&
            SceneUIController.Instance.BlockIfEditingScene())
            return;

        isOpen = willOpen;
    }
}
