using UnityEngine;

public class UI_DropdownPanel : MonoBehaviour
{
    public GameObject panel;

    private bool isOpen = false;

    void Start()
    {
        panel.SetActive(false); // ⭐ 开局隐藏
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);
    }
}
